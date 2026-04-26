# WorkTracker Frontend

Angular 21 single-page application for WorkTracker. The main project documentation lives in the repository root [`README.md`](../README.md).

## Scripts

Install dependencies:

```bash
npm ci
```

Run the development server:

```bash
npm start
```

The app is served at `http://localhost:4200` and calls the API at `http://localhost:5088/api/`.

Build the production bundle:

```bash
npm run build
```

Run unit tests:

```bash
npm test
```

No end-to-end runner is configured in this project.

## Application shape

- `src/app/core/api` - API endpoint constants
- `src/app/core/auth` - auth service, auth store, guards, and HTTP interceptors
- `src/app/features/auth` - login and registration pages
- `src/app/features/tasks` - task page and task API service
- `src/app/shared` - shared models and small UI utilities
- `src/environments` - build-time API base URL configuration

The production environment uses `/api/` as the API base URL. Kubernetes Ingress routes `/api` to the backend. The local Nginx config currently serves the SPA and `/healthz`; it does not proxy `/api/` by itself.
