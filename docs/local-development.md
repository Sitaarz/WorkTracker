# Local development

This document describes the simplest ways to run WorkTracker locally.

## Prerequisites

- .NET 10 SDK
- Node.js 24 and npm 11
- Docker and Docker Compose
- PostgreSQL 17, either local or containerized

## Recommended workflow

The most convenient workflow is to run PostgreSQL in Docker and run the backend
and frontend directly from source. This lets the Angular dev server call the API
at `http://localhost:5088/api/`.

### 1. Start PostgreSQL

```bash
docker run --name worktracker-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=worktracker \
  -p 5432:5432 \
  -d postgres:17-alpine
```

If the container already exists:

```bash
docker start worktracker-postgres
```

### 2. Apply migrations

```bash
cd backend/src/WorkTracker.API
dotnet restore
dotnet run --launch-profile http -- --migrate
```

### 3. Run API

```bash
cd backend/src/WorkTracker.API
dotnet run --launch-profile http
```

The API runs at `http://localhost:5088`.

In Development, these URLs are available:

- Swagger UI: `http://localhost:5088/swagger`
- OpenAPI JSON: `http://localhost:5088/openapi/v1.json`
- Health: `http://localhost:5088/health`

### 4. Run frontend

```bash
cd frontend
npm ci
npm start
```

The frontend runs at `http://localhost:4200`.

## Docker Compose

From the repository root:

```bash
docker compose up --build
```

Compose starts:

| Service | Role |
| --- | --- |
| `db` | PostgreSQL with persistent volume. |
| `migrate` | One-shot API container running EF Core migrations. |
| `api` | ASP.NET Core API on container port `8080`. |
| `web` | Angular production build served by Nginx on host port `8080`. |

Stop:

```bash
docker compose down
```

Stop and delete database volume:

```bash
docker compose down -v
```

Current caveat: production Angular builds use `/api/`, but the local
`frontend/nginx.conf` serves the SPA and `/healthz`; it does not currently proxy
`/api/` to the API container. For browser-driven local work, prefer the
recommended workflow with API on `:5088` and Angular dev server on `:4200`.

## Useful commands

Backend tests:

```bash
cd backend
dotnet test
```

Frontend tests:

```bash
cd frontend
npm test
```

Frontend production build:

```bash
cd frontend
npm run build
```

## Troubleshooting

| Symptom | Check |
| --- | --- |
| API cannot connect to DB | Confirm PostgreSQL is running and the connection string points to the right host. Outside Docker use `Host=localhost`; inside Compose/Kubernetes use the service name. |
| Login works but next request is unauthorized | Check cookie settings, request scheme and whether the browser sends credentials. In local Angular development the API CORS config must allow `http://localhost:4200`. |
| Integration tests hang or fail at startup | Make sure Docker is running because Testcontainers starts PostgreSQL. |
| Frontend calls wrong API URL | Check `frontend/src/environments/environment.ts` and `environment.prod.ts`. |
