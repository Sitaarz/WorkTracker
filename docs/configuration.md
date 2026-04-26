# Configuration

Backend configuration comes from `appsettings.json`,
`appsettings.{Environment}.json` and environment variables. Nested values use
double underscores in environment variable names, for example
`Jwt__SecretKey`.

The root `.env.example` documents expected variables for local/container use.
Do not commit a real `.env` file with secrets.

## Backend settings

| Setting | Example | Notes |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Development` enables Swagger/OpenAPI and development config. Use `Production` in deployed environments. |
| `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=worktracker;Username=postgres;Password=postgres` | PostgreSQL connection string. |
| `Jwt__Issuer` | `WorkTracker` | JWT issuer. |
| `Jwt__Audience` | `WorkTracker.Users` | JWT audience. |
| `Jwt__SecretKey` | strong random secret | Required, at least 32 UTF-8 bytes for HS256 signing. Replace outside local development. |
| `Jwt__ExpirationMinutes` | `60` | Access token lifetime in minutes. Must be greater than 0. |
| `Cors__AllowedOrigins__0` | `http://localhost:4200` | Needed when the SPA and API are served from different origins. |

## PostgreSQL settings

Docker and Kubernetes also need database initialization values:

| Setting | Example | Notes |
| --- | --- | --- |
| `POSTGRES_USER` | `postgres` or `worktracker` | User created by the PostgreSQL container. |
| `POSTGRES_PASSWORD` | secret value | Must match the password in the API connection string. |
| `POSTGRES_DB` | `worktracker` | Database created by the PostgreSQL container. |

## Frontend settings

The Angular app reads the API base URL at build time from environment files:

| File | Value |
| --- | --- |
| `frontend/src/environments/environment.ts` | `http://localhost:5088/api/` |
| `frontend/src/environments/environment.prod.ts` | `/api/` |

Runtime container environment variables do not change the built Angular bundle
unless the frontend is changed to support runtime config injection.

## Cookie and proxy-related settings

Auth uses an `HttpOnly` cookie named `access_token`.

Important behavior:

- The API sets `Secure = Request.IsHttps`.
- Forwarded headers are enabled, so behind Ingress or another TLS-terminating
  proxy the original HTTPS scheme can still be detected.
- For cross-origin local development, CORS must allow credentials and the
  frontend must send credentials.

## Local files

| File | Role |
| --- | --- |
| `.env.example` | Template and documentation for environment variables. |
| `backend/src/WorkTracker.API/appsettings.json` | Base API settings. |
| `backend/src/WorkTracker.API/appsettings.Development.json` | Local development overrides. |
| `docker-compose.yml` | Local container orchestration. Currently contains literal development values instead of `${VAR}` substitutions. |
| `k8s/10-config.yml` | Kubernetes non-secret config. |
| GitHub Actions secrets | Source of production secrets used during deployment. |

## Secret handling

Required production secrets:

| Secret | Used by |
| --- | --- |
| `DB_PASSWORD` | GitHub Actions creates Kubernetes DB/API secrets. |
| `JWT_SECRET` | JWT signing key. |
| `KUBE_CONFIG` | GitHub Actions access to Kubernetes cluster. |
| `GHCR_READ_TOKEN` | Optional long-lived package read token for private GHCR pulls. |

Rules:

- Never use development JWT secrets in shared or production environments.
- Keep DB password and JWT secret in the deployment secret store.
- Rotate secrets if they were printed in logs or committed accidentally.
