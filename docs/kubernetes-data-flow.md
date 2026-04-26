# Kubernetes data-flow diagram

Simple data-flow diagram for WorkTracker running in the `worktracker` namespace.

```mermaid
flowchart LR
    user["User<br/>browser"]
    dns["DNS<br/>krystian-sitarz.pl"]

    subgraph k8s[Kubernetes: namespace worktracker]
        ingress["Nginx Ingress<br/>TLS: worktracker-tls"]
        issuer["cert-manager Issuer<br/>letsencrypt-prod"]

        webSvc["Service worktracker-web<br/>80 -> 8080"]
        webPods["Deployment worktracker-web<br/>2x Nginx + Angular"]

        apiSvc["Service worktracker-api<br/>80 -> 8080"]
        apiPods["Deployment worktracker-api<br/>2x ASP.NET API"]

        migrate["Job worktracker-migrate<br/>API image + --migrate"]

        pgSvc["Service postgres<br/>ClusterIP :5432"]
        pgHeadless["Service postgres-headless<br/>StatefulSet DNS"]
        pg["StatefulSet postgres<br/>1x PostgreSQL 16.4"]
        pvc[(PVC postgres-storage<br/>5Gi)]

        appConfig["ConfigMap worktracker-config<br/>API/JWT settings"]
        appSecrets["Secret worktracker-secrets<br/>DB connection + JWT secret"]
        pgConfig["ConfigMap postgres-config<br/>database + user"]
        pgSecrets["Secret postgres-auth<br/>database password"]
        pullSecret["Secret ghcr-pull<br/>container image pulls"]
    end

    user -->|HTTPS| dns --> ingress
    issuer -. issues certificate .-> ingress

    ingress -->|GET / and static assets| webSvc --> webPods
    webPods -->|SPA HTML/CSS/JS| user

    user -->|/api/v1/... with access_token cookie| ingress
    user -->|/health| ingress
    ingress -->|/api and /health| apiSvc --> apiPods

    apiPods -->|SQL :5432| pgSvc --> pg
    pg -. persists data .-> pvc
    pgHeadless -. stable network identity .-> pg

    migrate -->|waits for DB and runs EF Core migrations| pgSvc

    appConfig -. envFrom .-> apiPods
    appSecrets -. envFrom .-> apiPods
    appConfig -. envFrom .-> migrate
    appSecrets -. envFrom .-> migrate

    pgConfig -. env .-> pg
    pgSecrets -. env .-> pg

    pullSecret -. imagePullSecrets .-> webPods
    pullSecret -. imagePullSecrets .-> apiPods
    pullSecret -. imagePullSecrets .-> migrate
```

## Main paths

1. The browser opens `https://krystian-sitarz.pl` or `https://www.krystian-sitarz.pl`.
2. The TLS-terminating Ingress routes `/` to `worktracker-web`, where Nginx serves the built Angular application.
3. The frontend uses the production `apiBaseUrl: /api/`, so API calls return through the same Ingress.
4. The Ingress routes `/api` and `/health` to `worktracker-api`.
5. The API reads configuration and secrets from `worktracker-config` and `worktracker-secrets`, and persists data in PostgreSQL through the `postgres` Service.
6. `worktracker-migrate` is a one-shot Job that runs the API image with the `--migrate` argument; it waits for PostgreSQL readiness before running migrations.
7. PostgreSQL runs as a `StatefulSet` and stores data on the `postgres-storage` volume.

## Source manifests

- `k8s/60-ingress.yml`
- `k8s/50-web.yml`
- `k8s/40-api.yml`
- `k8s/30-migrate-job.yml`
- `k8s/20-postgres.yml`
- `k8s/10-config.yml`
- `k8s/15-issuer.yml`
