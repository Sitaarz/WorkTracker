# WorkTracker documentation

This directory contains the technical documentation for WorkTracker. The root
`README.md` remains the project overview, while the files below describe
specific areas in more detail.

## Documents

| File | Contents |
| --- | --- |
| [mvp.md](./mvp.md) | MVP goal, user stories, and functional/nonfunctional requirements. |
| [architecture.md](./architecture.md) | Application layers, module responsibilities, and main flows. |
| [api.md](./api.md) | REST API endpoints, request/response contracts, and status codes. |
| [local-development.md](./local-development.md) | Local backend, frontend, and Docker Compose setup. |
| [configuration.md](./configuration.md) | Environment variables, JWT, CORS, and database connection configuration. |
| [database.md](./database.md) | Data model, EF Core migrations, and migration execution. |
| [testing.md](./testing.md) | Backend/frontend tests and minimal pre-merge checks. |
| [deployment.md](./deployment.md) | Docker, Kubernetes, GitHub Actions, and required secrets. |
| [kubernetes-data-flow.md](./kubernetes-data-flow.md) | Kubernetes data-flow diagram. |
| [decisions.md](./decisions.md) | Lightweight architecture decision log. |

## Quick path

1. New contributors should start with the root [README.md](../README.md).
2. Use [local-development.md](./local-development.md) to run the project locally.
3. For backend work, check [architecture.md](./architecture.md), [api.md](./api.md), and [database.md](./database.md).
4. For infrastructure changes, check [deployment.md](./deployment.md) and [kubernetes-data-flow.md](./kubernetes-data-flow.md).
5. Before submitting changes, run the tests described in [testing.md](./testing.md).
