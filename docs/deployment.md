# Deployment

WorkTracker can be built as two container images:

- API image from `backend/Dockerfile`
- Web image from `frontend/Dockerfile`

Production deployment in this repository targets Kubernetes through GitHub
Actions, GHCR images, Kustomize manifests and Nginx Ingress.

## Docker Compose

Local Compose command:

```bash
docker compose up --build
```

Services:

| Service | Role |
| --- | --- |
| `db` | PostgreSQL 17 with named volume. |
| `migrate` | Runs API image with `--migrate`, then exits. |
| `api` | ASP.NET Core API. |
| `web` | Nginx serving the Angular production build. |

Compose is useful for checking image builds and service startup. For active
browser development, see [local-development.md](./local-development.md).

## Kubernetes resources

Manifests live in `k8s/` and are applied through Kustomize.

| File | Resource |
| --- | --- |
| `00-namespace.yml` | `worktracker` namespace. |
| `10-config.yml` | ConfigMaps for API/PostgreSQL settings. |
| `15-issuer.yml` | cert-manager issuer. |
| `20-postgres.yml` | PostgreSQL StatefulSet, services and storage. |
| `30-migrate-job.yml` | EF Core migration Job. |
| `40-api.yml` | API Deployment and Service. |
| `50-web.yml` | Web Deployment and Service. |
| `60-ingress.yml` | Ingress for app, API and health routes. |

Ingress routes:

| Path | Target |
| --- | --- |
| `/` | `worktracker-web` |
| `/api` | `worktracker-api` |
| `/health` | `worktracker-api` |

The current hosts are:

- `krystian-sitarz.pl`
- `www.krystian-sitarz.pl`

See [kubernetes-data-flow.md](./kubernetes-data-flow.md) for the flow diagram.

## GitHub Actions

Workflow: `.github/workflows/deploy.yml`

Triggers:

- push to `main`
- manual `workflow_dispatch`

Pipeline summary:

1. Build API and web images.
2. Push images to GHCR with `sha-<shortsha>` and `latest` tags.
3. Configure `kubectl`, Helm and Kustomize.
4. Install or upgrade cert-manager.
5. Create/update Kubernetes secrets.
6. Rewrite image names/tags in Kustomize.
7. Delete old migration Job if present.
8. Apply manifests.
9. Wait for migration Job and API/web rollouts.

## Required secrets

| Secret | Purpose |
| --- | --- |
| `KUBE_CONFIG` | Kubeconfig used by the deployment workflow. |
| `DB_PASSWORD` | PostgreSQL password and API connection string password. |
| `JWT_SECRET` | JWT signing secret. Must be strong and at least 32 UTF-8 bytes. |
| `GHCR_READ_TOKEN` | Optional read token for private GHCR pulls. If absent, workflow falls back to `GITHUB_TOKEN`. |

## Manual deployment outline

From a machine with cluster access:

```bash
cd k8s
kustomize edit set image worktracker-api=<registry>/worktracker-api:<tag>
kustomize edit set image worktracker-web=<registry>/worktracker-web:<tag>
kubectl apply -k .
kubectl -n worktracker wait --for=condition=complete job/worktracker-migrate --timeout=5m
kubectl -n worktracker rollout status deploy/worktracker-api --timeout=5m
kubectl -n worktracker rollout status deploy/worktracker-web --timeout=5m
```

Make sure required secrets exist before applying manifests.

## Post-deployment checks

```bash
kubectl -n worktracker get pods
kubectl -n worktracker get ingress
kubectl -n worktracker logs deploy/worktracker-api --tail=100
kubectl -n worktracker logs deploy/worktracker-web --tail=100
```

HTTP checks:

- `GET https://krystian-sitarz.pl/`
- `GET https://krystian-sitarz.pl/health`
- `POST https://krystian-sitarz.pl/api/v1/auth/login`

## Rollback

The workflow tags every image with the commit short SHA. To roll back, set the
Kustomize image tags to a previous known-good SHA tag and apply the manifests.
Check whether the rollback also needs a database migration strategy; EF Core
migrations are normally forward-only.
