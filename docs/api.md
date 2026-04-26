# API reference

Base API path: `/api/v1`

In Development, OpenAPI is available at `/openapi/v1.json` and Swagger UI at
`/swagger`.

Authentication uses an `HttpOnly` cookie named `access_token`. Clients should
send credentials on protected requests.

## Common conventions

| Item | Convention |
| --- | --- |
| JSON enums | Serialized as strings, for example `ToDo`, `High`, `CreatedAt`. |
| Auth cookie | `access_token`, written by login/register and expired by logout. |
| Validation errors | HTTP `400` with `ValidationProblemDetails`. |
| Application errors | RFC 7807-style problem response. |
| Protected routes | Require a valid JWT in the auth cookie. |

## Auth endpoints

### Register

`POST /api/v1/auth/register`

Auth: public

Request:

```json
{
  "name": "Ada",
  "email": "ada@example.com",
  "password": "Password1"
}
```

Validation:

| Field | Rules |
| --- | --- |
| `name` | Required, max 100 characters. |
| `email` | Required, valid email address. |
| `password` | Required, min 6 characters, at least one uppercase letter, one lowercase letter and one digit. |

Success:

- `200 OK`
- Sets `access_token` cookie.

Response:

```json
{
  "user": {
    "email": "ada@example.com",
    "name": "Ada",
    "roles": ["User"]
  }
}
```

Errors:

- `400 Bad Request` for validation errors.
- `409 Conflict` when registration fails, for example duplicated email.

### Login

`POST /api/v1/auth/login`

Auth: public

Request:

```json
{
  "email": "ada@example.com",
  "password": "Password1"
}
```

Success:

- `200 OK`
- Sets `access_token` cookie.
- Response shape matches register.

Errors:

- `400 Bad Request` for validation errors.
- `401 Unauthorized` for invalid credentials.

### Current User

`GET /api/v1/auth/me`

Auth: required

Success:

- `200 OK`

Response:

```json
{
  "user": {
    "email": "ada@example.com",
    "name": "Ada",
    "roles": ["User"]
  }
}
```

Errors:

- `401 Unauthorized` when required JWT claims are missing or invalid.

### Logout

`POST /api/v1/auth/logout`

Auth: public

Success:

- `204 No Content`
- Expires the `access_token` cookie.

## Task endpoints

Task routes require authentication. A user can access only tasks owned by the
current JWT subject.

### Create Task

`POST /api/v1/tasks`

Request:

```json
{
  "title": "Prepare release notes",
  "description": "Collect changes and publish notes",
  "status": "ToDo",
  "priority": "Medium",
  "dueDate": "2026-05-01T00:00:00Z"
}
```

Validation:

| Field | Rules |
| --- | --- |
| `title` | Required, max 200 characters in API validation. |
| `description` | Required, max 2000 characters in API validation. |
| `status` | `ToDo`, `InProgress`, `Done`. |
| `priority` | `Low`, `Medium`, `High`. |
| `dueDate` | Nullable date/time. |

Success:

- `201 Created`

Response:

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "title": "Prepare release notes",
  "description": "Collect changes and publish notes",
  "status": "ToDo",
  "priority": "Medium",
  "dueDate": "2026-05-01T00:00:00Z",
  "ownerId": "22222222-2222-2222-2222-222222222222",
  "createdAt": "2026-04-26T18:00:00Z"
}
```

### List Tasks

`GET /api/v1/tasks`

Success:

- `200 OK`
- Returns an array of task DTOs for the current user.

### Get Task By Id

`GET /api/v1/tasks/{taskId}`

Success:

- `200 OK`
- Returns a single task DTO.

Errors:

- `404 Not Found` when task does not exist.
- `403 Forbidden` when the task belongs to another user.

### Update Task

`PUT /api/v1/tasks/{taskId}`

The `taskId` in the route must match `id` in the request body.

Request:

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "title": "Prepare release notes",
  "description": "Collect changes and publish notes",
  "status": "InProgress",
  "priority": "High",
  "dueDate": null
}
```

Success:

- `204 No Content`

Errors:

- `400 Bad Request` when route id and body id do not match or update fails.
- `404 Not Found` when task does not exist.
- `403 Forbidden` when the task belongs to another user.

### Delete Task

`DELETE /api/v1/tasks/{taskId}`

Success:

- `204 No Content`

Errors:

- `404 Not Found` when task does not exist.
- `403 Forbidden` when the task belongs to another user.

### Filter Tasks

`GET /api/v1/tasks/filter`

Query parameters:

| Parameter | Values | Default |
| --- | --- | --- |
| `status` | `ToDo`, `InProgress`, `Done` | none |
| `priority` | `Low`, `Medium`, `High` | none |
| `sortedBy` | `CreatedAt`, `DueDate` | `CreatedAt` |
| `sortDirection` | `Asc`, `Desc` | `Asc` |
| `page` | positive integer | `1` |
| `pageSize` | positive integer | `20` |

Example:

```text
GET /api/v1/tasks/filter?status=ToDo&priority=High&sortedBy=DueDate&sortDirection=Asc&page=1&pageSize=10
```

Success:

- `200 OK`

Response:

```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

## Health

`GET /health`

Auth: public

Success:

- `200 OK` when the API process is healthy.
