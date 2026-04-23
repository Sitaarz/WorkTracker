# WorkTracker

A lightweight task management application built around a Kanban-style board. Users can register, sign in, and manage their personal tasks with statuses, priorities, due dates, filtering, sorting and pagination.

The project is split into a .NET 10 REST API and an Angular 21 single-page application, and can be run end-to-end with a single `docker compose up`.

## Table of contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Repository layout](#repository-layout)
- [Architecture](#architecture)
- [API overview](#api-overview)
- [Getting started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Run with Docker Compose (recommended)](#run-with-docker-compose-recommended)
  - [Run locally (without Docker)](#run-locally-without-docker)
- [Configuration](#configuration)
- [Testing](#testing)
- [Database migrations](#database-migrations)
- [License](#license)

## Features

- User registration and login with JWT issued as an `HttpOnly` cookie
- Session refresh via `/auth/me` and logout that clears the auth cookie
- Task CRUD with per-user ownership checks (403 on cross-user access)
- Task attributes: title, description, status (`ToDo`, `InProgress`, `Done`), priority (`Low`, `Medium`, `High`), optional due date
- Filter, sort and paginate tasks via `GET /api/v1/tasks/filter`
- Centralized validation (FluentValidation) and RFC 7807 `ProblemDetails` error responses
- Global exception handler middleware
- Health endpoint (`/health`) used by Docker healthchecks
- OpenAPI / Swagger UI enabled in development
- SPA served by Nginx with API reverse-proxy under `/api/`

## Tech stack

**Backend**
- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 + Npgsql (PostgreSQL 17)
- JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- FluentValidation
- NUnit, NSubstitute and `Microsoft.NET.Test.Sdk` for tests

**Frontend**
- Angular 21 (standalone components, lazy-loaded routes)
- Angular Material and CDK
- RxJS, TypeScript 5.9
- Vitest for unit tests
- Nginx (`nginxinc/nginx-unprivileged:alpine`) for production hosting

**Infrastructure**
- Docker / Docker Compose for local orchestration
- `k8s/` directory reserved for Kubernetes manifests; the API supports `--migrate` for a dedicated migration Job

## Repository layout

```
WorkTracker/
├── backend/
│   ├── src/
│   │   ├── WorkTracker.API/            # ASP.NET Core host (controllers, middleware, DI composition)
│   │   ├── WorkTracker.Application/    # Use cases, commands/queries, validators, DTOs
│   │   ├── WorkTracker.Domain/         # Entities (User, TaskItem) and enums
│   │   └── WorkTracker.Infrastructure/ # EF Core DbContext, migrations, auth, persistence
│   ├── tests/
│   │   ├── WorkTracker.UnitTests/
│   │   └── WorkTracker.IntegrationTests/
│   ├── Dockerfile
│   └── WorkTracker.slnx
├── frontend/
│   ├── src/app/
│   │   ├── core/         # API client, auth service, guards, interceptors, layout
│   │   ├── features/     # auth/ and tasks/ feature modules (pages + components)
│   │   └── shared/       # Reusable UI, models, pipes, directives
│   ├── Dockerfile
│   ├── nginx.conf
│   └── package.json
├── docs/
│   └── mvp.md            # MVP scope and requirements
├── k8s/                  # (reserved) Kubernetes manifests
├── docker-compose.yml
├── LICENSE
└── README.md
```

## Architecture

The backend follows a Clean Architecture layout:

- **Domain** — pure entities and enums, no framework dependencies.
- **Application** — use-case handlers (commands/queries), FluentValidation validators, DTOs and mapping. References only Domain.
- **Infrastructure** — EF Core `DbContext`, migrations, identity/password hashing, JWT token generation. References Domain and Application abstractions.
- **API** — ASP.NET Core controllers, request contracts, mappers, middleware (global exception handler), DI composition root.

Authentication flow:
1. Client calls `POST /api/v1/auth/register` or `/login`.
2. API validates the request, persists or authenticates the user, generates a JWT, and writes it to an `HttpOnly` `access_token` cookie (`SameSite=Strict`, `Secure` outside Development).
3. Subsequent requests carry the cookie; the API reads `sub`, `name`, `email` and role claims to authorize actions and enforce ownership on tasks.

## API overview

All endpoints are versioned under `/api/v1`.

| Method | Route | Auth | Description |
| ------ | ----- | ---- | ----------- |
| POST   | `/api/v1/auth/register` | public | Register a new user, sets auth cookie |
| POST   | `/api/v1/auth/login`    | public | Log in, sets auth cookie |
| GET    | `/api/v1/auth/me`       | required | Return the current user from the JWT claims |
| POST   | `/api/v1/auth/logout`   | public | Clear the auth cookie |
| POST   | `/api/v1/tasks`         | required | Create a new task for the current user |
| GET    | `/api/v1/tasks`         | required | List all tasks owned by the current user |
| GET    | `/api/v1/tasks/{id}`    | required | Get a single task (403 if not the owner) |
| PUT    | `/api/v1/tasks/{id}`    | required | Update a task (403 if not the owner) |
| DELETE | `/api/v1/tasks/{id}`    | required | Delete a task (404/403 as appropriate) |
| GET    | `/api/v1/tasks/filter`  | required | Filter by `Status`/`Priority`, sort by `SortedBy`/`SortDirection`, paginate with `Page`/`PageSize` |
| GET    | `/health`               | public | Liveness / readiness probe |

When running in `Development`, Swagger UI is available at `/swagger` and the OpenAPI document at `/openapi/v1.json`.

## Getting started

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose (for the containerized workflow)
- [.NET 10 SDK](https://dotnet.microsoft.com/) (for local backend development)
- [Node.js 24+](https://nodejs.org/) and npm 11+ (for local frontend development)
- [PostgreSQL 17](https://www.postgresql.org/) if you want to run the database outside Docker

### Run with Docker Compose (recommended)

From the repository root:

```bash
docker compose up --build
```

This starts four services defined in `docker-compose.yml`:

- `db` — PostgreSQL 17, volume-backed, with `pg_isready` healthcheck
- `migrate` — one-shot container that runs `dotnet WorkTracker.API.dll --migrate` against the database and exits
- `api` — the ASP.NET Core API, started after migrations complete
- `web` — Nginx-served Angular app that reverse-proxies `/api/` to the API

Once healthy, open the app at http://localhost:8080. The Angular build targets `/api/` in production, so the SPA talks to the API through Nginx.

To stop and remove containers (but keep the `pgdata` volume):

```bash
docker compose down
```

### Run locally (without Docker)

**1. Database**

Start PostgreSQL and make sure it matches the connection string in `backend/src/WorkTracker.API/appsettings.json`, or override it with environment variables (see [Configuration](#configuration)).

**2. Backend**

```bash
cd backend/src/WorkTracker.API
dotnet restore
dotnet run
```

The API listens on the Kestrel defaults from `launchSettings.json` (the frontend's dev environment expects `http://localhost:5088`). CORS is enabled for `http://localhost:4200` in `appsettings.Development.json`.

**3. Frontend**

```bash
cd frontend
npm ci
npm start
```

The Angular dev server runs on http://localhost:4200 and calls the API at `http://localhost:5088/api/` (see `src/environments/environment.ts`).

## Configuration

The API reads configuration from `appsettings.json`, `appsettings.{Environment}.json` and environment variables. The most important keys are:

| Key | Description | Default |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=worktracker;Username=postgres;Password=postgres` |
| `Jwt__Issuer` | JWT issuer | `WorkTracker` |
| `Jwt__Audience` | JWT audience | `WorkTracker.Users` |
| `Jwt__SecretKey` | HMAC signing key (**replace in production**) | dev placeholder |
| `Jwt__ExpirationMinutes` | Access token lifetime | `60` |
| `Cors__AllowedOrigins__0` | Allowed CORS origin for the SPA | `http://localhost:4200` (dev only) |
| `ASPNETCORE_ENVIRONMENT` | `Development` enables Swagger and relaxes cookie `Secure` flag | — |

The frontend's API base URL is controlled by `frontend/src/environments/environment*.ts`.

## Testing

**Backend**

```bash
cd backend
dotnet test
```

This runs both `WorkTracker.UnitTests` (NUnit + NSubstitute) and `WorkTracker.IntegrationTests` (uses a shared `IntegrationTestFixture` to boot the API in-memory).

**Frontend**

```bash
cd frontend
npm test
```

Angular CLI is configured to use Vitest as the test runner.

## Database migrations

EF Core migrations live in `backend/src/WorkTracker.Infrastructure/Migrations`. They are applied automatically at startup when the API is launched with the `--migrate` flag:

```bash
dotnet WorkTracker.API.dll --migrate
```

In Docker Compose this is handled by the dedicated `migrate` service, which runs before the `api` container starts. The same pattern is designed to work as a Kubernetes migration `Job`.

To add a new migration during development:

```bash
cd backend
dotnet ef migrations add <Name> \
  --project src/WorkTracker.Infrastructure \
  --startup-project src/WorkTracker.API
```

## License

Released under the [MIT License](./LICENSE). Copyright (c) Krystian Sitarz.
