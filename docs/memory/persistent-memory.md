# Persistent Memory Storage
Atlas uses SQLite and Entity Framework Core for persistent memory storage.

This document describes how the development environment initializes the memory database, how schema changes are introduced, and how developers can reset their local database when needed.

## Overview
Persistent memory is owned by `Atlas.Memory`.

The database lifecycle is split into two responsibilities:

* `Atlas.Memory` owns the database model, migrations, storage implementation, and initialization abstraction.
* `Atlas.Hosting` invokes memory initialization during application startup.

The startup flow is:

```text
Atlas starts
    ↓
AtlasMemoryInitializationHostedService
    ↓
IAtlasMemoryStoreInitializer
    ↓
EntityFrameworkAtlasMemoryStoreInitializer
    ↓
DbContext.Database.MigrateAsync()
    ↓
pending EF Core migrations are applied
    ↓
AtlasRuntimeHostedService
    ↓
Atlas runtime starts
```

Database initialization does not belong in interaction handlers, command handlers, or other application-level request processing.

## Development configuration
Application configuration is owned by `Atlas.Hosting`.

The development SQLite configuration is:

```json
{
  "Atlas": {
    "Name": "Atlas",
    "Memory": {
      "StorageMode": "Sqlite"
    },
    "Interaction": {
      "InterpreterMode": "Deterministic"
    }
  },
  "ConnectionStrings": {
    "AtlasMemory": "Data Source=atlas.db"
  }
}
```

This configuration belongs in:

```text
src/Atlas.Hosting/appsettings.json
```

The `Atlas.Memory` project does not contain application `appsettings.json` files.

When `StorageMode` is `Sqlite`, `Atlas.Memory` uses the `AtlasMemory` connection string and registers:

```text
IAtlasMemoryStore
    → EntityFrameworkAtlasMemoryStore

IAtlasMemoryStoreInitializer
    → EntityFrameworkAtlasMemoryStoreInitializer
```

When `StorageMode` is `InMemory`, no database is created or modified.

## First startup
A fresh development installation does not require manually creating the SQLite database.

When Atlas starts with SQLite enabled:

1. The SQLite database is created when necessary.
2. Entity Framework Core checks the migration history.
3. Any pending migrations are applied.
4. Atlas continues startup.
5. The runtime starts after memory initialization has completed.

For a completely fresh installation, the initial migration creates the `Memories` table and its indexes.

The migration history is maintained by Entity Framework Core in:

```text
__EFMigrationsHistory
```

The current initial migration is:

```text
InitialCreate
```

## Creating a new migration
When the persistent memory model changes, create a new EF Core migration.

Run the following command from the repository root:

```powershell
dotnet ef migrations add DescribeTheChange `
    --project src/Atlas.Memory `
    --startup-project src/Atlas.Hosting `
    --context AtlasMemoryDbContext `
    --output-dir Migrations
```

Replace `DescribeTheChange` with a meaningful migration name, for example:

```powershell
dotnet ef migrations add AddMemoryUpdatedAt `
    --project src/Atlas.Memory `
    --startup-project src/Atlas.Hosting `
    --context AtlasMemoryDbContext `
    --output-dir Migrations
```

The migration files are generated under:

```text
src/Atlas.Memory/Migrations/
```

A migration normally consists of:

```text
<timestamp>_MigrationName.cs
AtlasMemoryDbContextModelSnapshot.cs
```

The generated migration files should be reviewed before committing them.

> [!IMPORTANT]
> Do not manually edit generated migration files unless there is a deliberate reason to do so and the resulting schema has been verified.

## Applying migrations during development
Developers normally do not need to run a separate database-update command.

Atlas applies pending migrations automatically during startup through:

```csharp
Database.MigrateAsync()
```

This means the normal development workflow is:

```text
1. Change the EF Core model
2. Generate a migration
3. Review the generated migration
4. Commit the migration
5. Start Atlas
6. Atlas applies the pending migration automatically
```

An existing development database is therefore updated incrementally rather than recreated.

## Fresh database reset
When a completely clean development database is required, the local SQLite database can be deleted.

With the default development connection string, remove:

```text
atlas.db
```

If SQLite has created companion files, remove those as well:

```text
atlas.db-wal
atlas.db-shm
```

On the next Atlas startup, the database is recreated and all migrations are applied from the beginning.

Deleting the local database permanently removes the memories stored in that development database. This should therefore only be done when resetting the development environment is intentional.

## Migrations versus memory data
Migrations describe the **database schema**, not Atlas memory contents.

```text
EF Core migrations
    ↓
database structure and schema history

Atlas memory store
    ↓
actual persisted memory data
```

For example, `InitialCreate` defines the `Memories` table and its indexes. It does not contain Atlas memories that should be restored on every installation.

## Do not use EnsureCreated for the application database
Production and development application startup use EF Core migrations.

Do not replace the migration-based initialization with:

```csharp
Database.EnsureCreated();
```

`EnsureCreated` creates a database directly from the current EF model and does not use the migrations history. It is therefore not the mechanism used by Atlas for the persistent memory database.

Migration-based initialization is required so that future schema changes can be applied incrementally.

## Testing
Persistent memory initialization is independently testable.

The following areas are covered by automated tests:

* in-memory initialization
* SQLite initializer behavior
* cancellation handling
* dependency-injection provider selection
* migration application against a fresh SQLite database
* repeated initialization against an already-migrated database
* Atlas hosting startup integration

The hosting integration test verifies that Atlas can start against a fresh SQLite database and that the `InitialCreate` migration is applied before normal runtime startup.

Run the complete test suite with:

```powershell
dotnet test
```

## EF Core tooling
The `Atlas.Memory` project references the EF Core Design package for migration tooling.

`Atlas.Hosting` also references the EF Core Design package because it is the startup project used by the EF Core command-line tools.

The `dotnet-ef` tool version should match the EF Core version used by the solution.

For example:

```powershell
dotnet-ef --version
```

The migration commands in this document assume the command is executed from the repository root.

## Development workflow summary
For normal persistent-memory development:

```text
Modify memory model
        ↓
Generate migration
        ↓
Review migration
        ↓
Run tests
        ↓
Commit migration
        ↓
Start Atlas
        ↓
Pending migrations applied automatically
```

For a clean local reset:

```text
Stop Atlas
    ↓
Delete atlas.db
    ↓
Start Atlas
    ↓
All migrations applied
    ↓
Fresh empty database
```

This approach keeps persistent memory initialization centralized, migration-driven, testable, and independent from Atlas interaction and command processing.
