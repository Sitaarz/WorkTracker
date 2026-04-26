# WorkTracker

WorkTracker is a personal task management application for authenticated users. It combines a .NET 10 REST API, an Angular 21 single-page app, and PostgreSQL persistence.

The current product is a list/form-based task tracker with task statuses, priorities, due dates, filtering, sorting, and pagination. Drag-and-drop Kanban is not implemented yet.

## Contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Repository layout](#repository-layout)
- [Architecture](#architecture)
- [API overview](#api-overview)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [Testing](#testing)
- [Database migrations](#database-migrations)
- [Kubernetes and deployment](#kubernetes-and-deployment)
- [Current notes](#current-notes)
- [License](#license)

## Features

- User registration, login, session bootstrap, and logout
- JWT authentication stored in an `HttpOnly` `access_token` cookie
- Angular route guards and HTTP interceptors that send cookie credentials
- Per-user task ownership checks in the API
- Task create, read, update, and delete operations
- Task fields: title, description, status (`ToDo`, `InProgress`, `Done`), priority (`Low`, `Medium`, `High`), optional due date, creation date
- Task filtering by status and priority
- Task sorting by `CreatedAt` or `DueDate`, ascending or descending
- Task pagination through the filtered endpoint
- FluentValidation request validation and RFC 7807-style problem responses
- Global exception handling middleware
- Health endpoint at `/health`
- OpenAPI document and Swagger UI in `Development`
- EF Core migration mode through the `--migrate` app argument
- Unit and integration tests for backend behavior
- Angular unit tests with Vitest
- Dockerfiles, Docker Compose, Kubernetes manifests, and a GitHub Actions deployment workflow

## Tech stack

**Backend**

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 with Npgsql
- PostgreSQL
- JWT Bearer authentication reading tokens from cookies
- ASP.NET Core Identity password hashing
- FluentValidation
- NUnit, NSubstitute, Testcontainers for PostgreSQL, Respawn

**Frontend**

- Angular 21 standalone application
- Angular Signals, Reactive Forms, Router, and HttpClient interceptors
- RxJS and TypeScript 5.9
- SCSS with custom component styles
- Vitest through the Angular test builder
- Nginx unprivileged image for production static hosting

**Infrastructure**

- Multi-stage Docker builds for API and web
- Docker Compose for local container orchestration
- Kubernetes manifests managed by Kustomize
- cert-manager issuer and Nginx Ingress routing in Kubernetes
- GitHub Actions deployment to Kubernetes with GHCR images

## Repository layout

```text
WorkTracker/
|-- backend/
|   |-- src/
|   |   |-- WorkTracker.API/             # ASP.NET Core host, controllers, contracts, mappers, middleware
|   |   |-- WorkTracker.Application/     # Use cases, validators, DTOs, abstractions
|   |   |-- WorkTracker.Domain/          # User and TaskItem entities, task enums, roles
|   |   `-- WorkTracker.Infrastructure/  # EF Core, repositories, migrations, JWT, DI
|   |-- tests/
|   |   |-- WorkTracker.UnitTests/
|   |   `-- WorkTracker.IntegrationTests/
|   |-- Dockerfile
|   `-- WorkTracker.slnx
|-- frontend/
|   |-- src/app/
|   |   |-- core/                        # API endpoints, auth service/store/guards/interceptors
|   |   |-- features/                    # Auth pages and task page/service
|   |   `-- shared/                      # Shared error models and toast service
|   |-- Dockerfile
|   |-- nginx.conf
|   `-- package.json
|-- docs/
|   `-- mvp.md
|-- k8s/                                # Namespace, config, Postgres, migrate job, API/web, ingress
|-- .github/
|   `-- workflows/deploy.yml
|-- .env.example
|-- docker-compose.yml
|-- LICENSE
`-- README.md
```

## Architecture

The backend follows a layered Clean Architecture style:

- **Domain** - framework-light entities and enums.
- **Application** - commands, queries, validators, DTOs, result objects, and persistence/auth abstractions.
- **Infrastructure** - EF Core `DbContext`, repositories, migrations, JWT generation, authentication setup.
- **API** - ASP.NET Core controllers, request contracts, mappers, middleware, CORS, OpenAPI, health checks, and composition root.

Authentication flow:

1. The client calls `POST /api/v1/auth/register` or `POST /api/v1/auth/login`.
2. The API validates input, creates or authenticates the user, generates a JWT, and writes it to an `HttpOnly` cookie named `access_token`.
3. The cookie uses `SameSite=Lax`; `Secure` is enabled when the request is HTTPS. Forwarded headers are enabled so this also works behind TLS-terminating proxies.
4. Angular sends requests with `withCredentials: true`.
5. The API reads the JWT from the cookie, validates claims, and enforces task ownership for protected task operations.

## API overview

All API endpoints are under `/api/v1`. Swagger UI is available at `/swagger` in `Development`, with the OpenAPI JSON at `/openapi/v1.json`.

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/v1/auth/register` | Public | Register a user and set the auth cookie |
| `POST` | `/api/v1/auth/login` | Public | Log in and set the auth cookie |
| `GET` | `/api/v1/auth/me` | Required | Return the current user from JWT claims |
| `POST` | `/api/v1/auth/logout` | Public | Expire the auth cookie |
| `POST` | `/api/v1/tasks` | Required | Create a task for the current user |
| `GET` | `/api/v1/tasks` | Required | List all tasks for the current user |
| `GET` | `/api/v1/tasks/{taskId}` | Required | Get a single task; returns 403 for another user's task |
| `PUT` | `/api/v1/tasks/{taskId}` | Required | Update a task; URL id must match body id |
| `DELETE` | `/api/v1/tasks/{taskId}` | Required | Delete a task |
| `GET` | `/api/v1/tasks/filter` | Required | Filter, sort, and paginate tasks |
| `GET` | `/health` | Public | API health probe |

Filtered task query parameters:

| Parameter | Values | Default |
| --- | --- | --- |
| `Status` | `ToDo`, `InProgress`, `Done` | none |
| `Priority` | `Low`, `Medium`, `High` | none |
| `SortedBy` | `CreatedAt`, `DueDate` | `CreatedAt` |
| `SortDirection` | `Asc`, `Desc` | `Asc` |
| `Page` | positive integer | `1` |
| `PageSize` | positive integer | `20` |

## Getting started

### Prerequisites

- .NET 10 SDK
- Node.js 24 and npm 11
- Docker and Docker Compose
- PostgreSQL 17 for local development outside containers, or another reachable PostgreSQL instance

### Local development

This is the most reliable workflow for browser-based development because the Angular dev server calls the API directly at `http://localhost:5088/api/`.

**1. Start PostgreSQL**

Use a local PostgreSQL instance that matches the development connection string:

```bash
docker run --name worktracker-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=worktracker \
  -p 5432:5432 \
  -d postgres:17-alpine
```

If the container already exists, start it with:

```bash
docker start worktracker-postgres
```

**2. Apply migrations and run the API**

```bash
cd backend/src/WorkTracker.API
dotnet restore
dotnet run --launch-profile http -- --migrate
dotnet run --launch-profile http
```

The HTTP launch profile listens on `http://localhost:5088`. Development CORS allows `http://localhost:4200`.

**3. Run the frontend**

```bash
cd frontend
npm ci
npm start
```

Open `http://localhost:4200`.

### Docker Compose

From the repository root:

```bash
docker compose up --build
```

Compose starts:

- `db` - PostgreSQL 17 with a named volume
- `migrate` - one-shot API image running `--migrate`
- `api` - ASP.NET Core API on container port `8080`
- `web` - Nginx-served Angular production build on host port `8080`

Stop containers with:

```bash
docker compose down
```

Remove the database volume as well:

```bash
docker compose down -v
```

See [Current notes](#current-notes) for the current local Compose API-routing caveat.

## Configuration

The API reads `appsettings.json`, `appsettings.{Environment}.json`, and environment variables. Nested settings use double underscores in environment variable names.

| Setting | Used by | Notes |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | API, migration job | PostgreSQL connection string. Development uses `Host=localhost;Port=5432;Database=worktracker;Username=postgres;Password=postgres`. |
| `Jwt__Issuer` | API | JWT issuer, default `WorkTracker`. |
| `Jwt__Audience` | API | JWT audience, default `WorkTracker.Users`. |
| `Jwt__SecretKey` | API | HMAC signing key. Must be at least 32 UTF-8 bytes and must be changed outside local development. |
| `Jwt__ExpirationMinutes` | API | JWT lifetime in minutes, default `60`. |
| `Cors__AllowedOrigins__0` | API | Development SPA origin, usually `http://localhost:4200`. |
| `ASPNETCORE_ENVIRONMENT` | API | `Development` enables OpenAPI/Swagger and development config. |
| `environment.apiBaseUrl` | Frontend | Build-time Angular setting. Dev: `http://localhost:5088/api/`; prod: `/api/`. |

`.env.example` documents the expected variables, but the current `docker-compose.yml` still contains literal development values. Convert those literals to `${VARIABLE}` references if you want Compose to load values from a local `.env` file.

## Testing

Backend:

```bash
cd backend
dotnet test
```

The integration test project starts a real PostgreSQL database through Testcontainers and resets database state with Respawn, so Docker must be running.

Frontend:

```bash
cd frontend
npm test
```

Production build check:

```bash
cd frontend
npm run build
```

No end-to-end test runner is configured at the moment.

## Database migrations

EF Core migrations live in `backend/src/WorkTracker.Infrastructure/Migrations`.

Apply migrations in local development:

```bash
cd backend/src/WorkTracker.API
dotnet run --launch-profile http -- --migrate
```

Apply migrations from a published/containerized API image:

```bash
dotnet WorkTracker.API.dll --migrate
```

Add a migration:

```bash
cd backend
dotnet ef migrations add <Name> \
  --project src/WorkTracker.Infrastructure \
  --startup-project src/WorkTracker.API
```

The Docker Compose `migrate` service and Kubernetes `worktracker-migrate` Job both use the same `--migrate` path before the API starts serving traffic.

## Kubernetes and deployment

The `k8s/` directory contains a Kustomize deployment for the `worktracker` namespace:

- ConfigMaps for API and PostgreSQL settings
- cert-manager `Issuer` for Let's Encrypt
- PostgreSQL `StatefulSet` and services
- API migration `Job`
- API and web `Deployment` and `Service` resources
- Ingress for `krystian-sitarz.pl` and `www.krystian-sitarz.pl`, routing `/api` and `/health` to the API and `/` to the web app

`.github/workflows/deploy.yml` builds API and web images, pushes them to GHCR, updates the Kustomize image tags, creates required Kubernetes secrets, installs cert-manager, applies the manifests, waits for migrations, and watches API/web rollout status.

Required repository secrets for deployment:

- `KUBE_CONFIG`
- `DB_PASSWORD`
- `JWT_SECRET`
- `GHCR_READ_TOKEN` for private GHCR pulls; if absent, the workflow falls back to `GITHUB_TOKEN`

## Current notes

- The frontend currently presents tasks as a list with create/edit/filter forms. It does not yet provide a drag-and-drop Kanban board.
- The production Angular build uses `/api/` as its API base URL. Kubernetes Ingress routes that correctly. The local `frontend/nginx.conf` currently serves the SPA and `/healthz`, but does not proxy `/api/` to the API container, and `docker-compose.yml` does not publish the API port. Use the local development workflow above for a full browser-driven setup until the Compose proxy/port wiring is added.
- The root `.env.example` is useful documentation, but Compose will not consume those values until `docker-compose.yml` is changed to use environment-variable substitutions.

## License

Released under the [MIT License](./LICENSE). Copyright (c) Krystian Sitarz.
