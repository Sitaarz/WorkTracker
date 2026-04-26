# Testing

WorkTracker has backend unit tests, backend integration tests and frontend unit
tests.

## Backend

Run all backend tests:

```bash
cd backend
dotnet test
```

Test projects:

| Project | Role |
| --- | --- |
| `tests/WorkTracker.UnitTests` | Fast tests for domain and application behavior. |
| `tests/WorkTracker.IntegrationTests` | API/infrastructure tests with real PostgreSQL via Testcontainers. |

Integration test notes:

- Docker must be running.
- Testcontainers starts PostgreSQL automatically.
- Respawn resets database state between tests.
- `CustomWebApplicationFactory` wires the API host for tests.

## Frontend

Install dependencies:

```bash
cd frontend
npm ci
```

Run unit tests:

```bash
npm test
```

The Angular test builder uses Vitest.

Run production build check:

```bash
npm run build
```

## Suggested pre-merge checklist

Before merging application changes:

```bash
cd backend
dotnet test
```

```bash
cd frontend
npm test
npm run build
```

For API contract changes, also smoke-test:

- register
- login
- `GET /api/v1/auth/me`
- create task
- update task
- delete task
- filter tasks

## Coverage expectations

Add or update tests when a change affects:

- validation rules
- authentication or authorization behavior
- task ownership checks
- repository filtering, sorting or pagination
- migrations or persistence mappings
- route guards/interceptors
- user-visible frontend flows

No end-to-end test runner is configured at the moment.
