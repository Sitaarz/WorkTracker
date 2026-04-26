# Architecture decisions

This file is a lightweight record of architecture decisions. Full ADRs can be
added later if the project needs a more formal process.

## 2026-04-26: Layered backend structure

Decision: backend is split into API, Application, Domain and Infrastructure
projects.

Reason:

- API concerns stay outside use-case logic.
- Application layer can depend on abstractions instead of EF Core/JWT details.
- Domain entities remain small and framework-light.
- Tests can target application/domain behavior without starting the whole host.

Trade-off:

- More projects and mapping code than in a single-project CRUD API.

## 2026-04-26: JWT stored in HttpOnly cookie

Decision: login/register write the JWT to an `HttpOnly` `access_token` cookie.

Reason:

- JavaScript does not need to read or store the token.
- Angular can authenticate API calls by sending credentials.
- Same-origin production deployment through Ingress keeps the frontend/API
  integration simple.

Trade-off:

- Cookie auth requires careful CORS and proxy configuration in local and
  deployed environments.

## 2026-04-26: EF Core migrations run as separate mode

Decision: the API supports `--migrate`, which applies migrations and exits.

Reason:

- Docker Compose and Kubernetes can run migrations before normal API startup.
- The same application image owns schema changes.
- The web process does not need to run migrations on every startup path.

Trade-off:

- Deployment must coordinate the migration Job/service and API rollout.

## 2026-04-26: Production frontend uses `/api/`

Decision: production Angular environment uses relative API base URL `/api/`.

Reason:

- Browser calls go through the same origin as the SPA.
- Kubernetes Ingress can route `/api` to the API service and `/` to the web
  service.
- Cookie auth is simpler when frontend and API share the public origin.

Trade-off:

- Local production-style Docker Compose needs an Nginx proxy for `/api/` if it
  should support full browser flows.

## 2026-04-26: Integration tests use Testcontainers

Decision: integration tests start PostgreSQL through Testcontainers instead of
using a shared local database.

Reason:

- Tests are closer to production persistence behavior.
- Local database state does not leak into tests.
- CI can create an isolated database per test run.

Trade-off:

- Docker is required to run integration tests.
