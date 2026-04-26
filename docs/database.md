# Database

WorkTracker uses PostgreSQL through Entity Framework Core and Npgsql. Migrations
live in `backend/src/WorkTracker.Infrastructure/Migrations`.

## Main entities

### User

Source: `backend/src/WorkTracker.Domain/Entities/User.cs`

| Field | Notes |
| --- | --- |
| `Id` | User identifier. |
| `Name` | Display name. |
| `Email` | Login email. Registration checks for an existing email before creating a user. |
| `Role` | Defaults to `User`. |
| `PasswordHash` | Stored password hash, never raw password. |
| `CreatedAt` | Creation timestamp. |
| `UpdatedAt` | Update timestamp. |
| `TaskItems` | One-to-many relation to tasks. |

### TaskItem

Source: `backend/src/WorkTracker.Domain/Entities/TaskItem.cs`

| Field | Notes |
| --- | --- |
| `Id` | Task identifier. |
| `Title` | Required task title. |
| `Description` | Required task description. |
| `Status` | `ToDo`, `InProgress`, `Done`. |
| `Priority` | `Low`, `Medium`, `High`. |
| `DueDate` | Nullable due date. |
| `OwnerId` | Owning user id. |
| `CreatedAt` | Creation timestamp in UTC. |
| `Owner` | Navigation property to user. |

## Migrations

Apply migrations locally:

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

Review migration output before committing:

- migration file in `WorkTracker.Infrastructure/Migrations`
- updated `WorkTrackerDbContextModelSnapshot.cs`
- generated column names, indexes and cascade behavior

## Runtime migration strategy

The API supports a `--migrate` argument. When present, the app:

1. Builds the service provider.
2. Resolves `WorkTrackerDbContext`.
3. Runs `Database.MigrateAsync()`.
4. Exits without starting the web host.

This mode is used by:

- Docker Compose `migrate` service
- Kubernetes `worktracker-migrate` Job

The normal API container starts only after migrations complete successfully in
the current Compose/Kubernetes configuration.

## Local database reset

When using the standalone local PostgreSQL container:

```bash
docker rm -f worktracker-postgres
docker volume prune
```

Be careful with `docker volume prune`: it removes unused Docker volumes, not
only WorkTracker data.

When using Docker Compose:

```bash
docker compose down -v
```

## Integration tests

Integration tests do not reuse the local development database. They start a
temporary PostgreSQL instance through Testcontainers and reset state with
Respawn.
