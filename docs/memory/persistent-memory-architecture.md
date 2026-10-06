# Persistent Memory Architecture
This document describes the persistent memory architecture introduced in milestone: Persistent Memory.

It defines the responsibilities and dependency boundaries between the Atlas memory capability, storage providers, persistence models, application configuration, database initialization, and integration-test infrastructure.

For day-to-day development instructions such as creating migrations, resetting a local database, and running persistence tests, see the persistent memory development guide.

## Overview
Atlas separates the **memory capability** from the **mechanism used to store memories**.

The application interacts with memory through `IAtlasMemory`.

`AtlasMemory` provides the memory capability and delegates persistence operations to `IAtlasMemoryStore`.

The configured storage provider implements `IAtlasMemoryStore`.

The resulting architecture is:
```text
Application / Interaction / Commands
                │
                ▼
           IAtlasMemory
                │
                ▼
           AtlasMemory
          (capability facade)
                │
                ▼
       IAtlasMemoryStore
             /     \
            /       \
           ▼         ▼
InMemoryAtlasMemoryStore
                    EntityFrameworkAtlasMemoryStore
                              │
                              ▼
                         SQLite / EF Core
```

This separation allows storage technology to change without changing the command or interaction behavior of Atlas.

## Memory capability versus storage
`IAtlasMemory` is the application-facing memory abstraction.

It provides operations such as:

```csharp
Task StoreAsync(
    AtlasMemoryEntry memory,
    CancellationToken cancellationToken = default);

Task<AtlasMemoryEntry?> GetAsync(
    Guid id,
    CancellationToken cancellationToken = default);

Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
    string query,
    CancellationToken cancellationToken = default);

Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
    AtlasMemoryQuery query,
    CancellationToken cancellationToken = default);
```

`AtlasMemory` implements this capability.

It is intentionally not responsible for deciding where memories are stored.

Instead, `AtlasMemory` delegates persistence to
`
IAtlasMemoryStore
`
.

This means the application can continue using `IAtlasMemory` without knowing whether memory is stored in memory, SQLite, or another future storage system.

## Storage abstraction
`IAtlasMemoryStore` defines the operations required by a storage provider:

```csharp
Task StoreAsync(
    AtlasMemoryEntry memory,
    CancellationToken cancellationToken = default);

Task<AtlasMemoryEntry?> GetAsync(
    Guid id,
    CancellationToken cancellationToken = default);

Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
    AtlasMemoryQuery query,
    CancellationToken cancellationToken = default);
```

The storage abstraction operates on Atlas memory models rather than database-specific entities.

The current implementations are:

```text
IAtlasMemoryStore
    ├── InMemoryAtlasMemoryStore
    └── EntityFrameworkAtlasMemoryStore
```

### In-memory storage
`InMemoryAtlasMemoryStore` stores memories in an in-process dictionary.

It is intended for:

* development scenarios where persistence is not required
* lightweight execution
* tests that do not require durable storage

Data stored by this provider is lost when the application process or service lifetime ends.

### SQLite storage
`EntityFrameworkAtlasMemoryStore` stores memories using Entity Framework Core and SQLite.

It is intended for Atlas's local-first persistent memory scenario.

The store uses `IDbContextFactory<AtlasMemoryDbContext>` so that database contexts remain short-lived and the store itself can be registered as a singleton.

The application does not interact with `DbContext` directly.

## Atlas memory model versus persistence model
Atlas intentionally separates its in-memory/domain-facing memory model from its database persistence model.

### Atlas memory model

The application-facing model is:

```text
AtlasMemoryEntry
    ├── Id
    ├── Content
    ├── CreatedAt
    ├── Type
    └── Interpretation
```

`AtlasMemoryEntry` represents an Atlas memory and can be used throughout the application without exposing database concerns.

### Persistence model
The database representation is:

```text
AtlasMemoryRecord
    ├── Id
    ├── Content
    ├── CreatedAt
    ├── Type
    ├── InterpretationType
    └── InterpretationData
```

`AtlasMemoryRecord` is specifically designed for persistence.

The persistence model stores interpreted data using a type discriminator and serialized data:

```text
InterpretationType
InterpretationData
```

This allows the database representation to remain stable even as Atlas gains additional memory interpretation types.

### Mapping
`AtlasMemoryRecordMapper` translates between the two models:

```text
AtlasMemoryEntry
        ↕
AtlasMemoryRecordMapper
        ↕
AtlasMemoryRecord
```

Storage implementations are therefore responsible for persistence concerns without forcing database-specific concepts into the application memory model.

## Interpretation persistence
`AtlasMemoryEntry.Interpretation` may contain an `IAtlasMemoryData` implementation.

The persistence model cannot directly store an interface instance, so the data is converted into:

```text
InterpretationType
InterpretationData
```

For example:

```text
AtlasTaskData
    ↓
InterpretationType = Task
InterpretationData = serialized task data
```

The mapper reconstructs the Atlas interpretation when the record is read.

Unsupported interpretation data types are rejected explicitly rather than silently discarded.

This makes future memory-data extensions visible and deliberate.

## Database responsibility boundaries
The persistent-memory architecture divides responsibility as follows.

### `Atlas.Memory`
Owns:
- memory abstractions
- memory models
- memory persistence models
- persistence mapping
- storage implementations
- database context
- storage initialization abstraction
- EF Core migrations

### `Atlas.Hosting`
Owns:
- application configuration
- startup orchestration
- invocation of memory initialization during application startup

`Atlas.Hosting` does not contain EF Core migration logic.

It invokes `IAtlasMemoryStoreInitializer` instead.

### Commands and interaction
`Atlas.Commands` and `Atlas.Interaction` do not know about:

- SQLite
- Entity Framework Core
- `DbContext`
- migrations
- database files
- persistence configuration

They interact with `IAtlasMemory` and therefore remain independent from storage technology.

This is a critical architectural boundary.

## Initialization and migrations
Persistent storage initialization is separated from normal memory operations.

The abstraction is `IAtlasMemoryStoreInitializer`.

Current implementations are:

```text
IAtlasMemoryStoreInitializer
    ├── InMemoryAtlasMemoryStoreInitializer
    └── EntityFrameworkAtlasMemoryStoreInitializer
```

The in-memory implementation performs no work because no schema exists.

The Entity Framework implementation creates a short-lived context and calls:

```csharp
Database.MigrateAsync()
```

This means startup can handle both:

```text
Fresh installation
    ↓
create database
    ↓
apply all migrations
```

and:

```text
Existing installation
    ↓
detect pending migrations
    ↓
apply schema changes
```

The hosted-service startup sequence is:

```text
AtlasMemoryInitializationHostedService
                ↓
    IAtlasMemoryStoreInitializer
                ↓
        database initialization
                ↓
AtlasRuntimeHostedService
                ↓
           Atlas starts
```

This ensures Atlas does not start normal runtime processing before persistent memory initialization has completed.

Database initialization is therefore not performed inside:

- command handlers
- interaction handlers
- memory commands
- interaction processors

## Migrations are the schema history
Entity Framework migrations define the evolution of the persistence schema.

The current initial migration is:

```text
InitialCreate
```

The database also contains Entity Framework's migration history:

```text
__EFMigrationsHistory
```

Future schema changes are represented by additional migrations.

The application startup process applies pending migrations automatically through `MigrateAsync()`.

`EnsureCreated()` is not used for the application database because it bypasses the migration model.

This separation allows Atlas to evolve the persistence schema incrementally.

## Configuration
Application configuration belongs to `Atlas.Hosting`.

The memory provider is selected using:

```json
{
  "Atlas": {
    "Memory": {
      "StorageMode": "InMemory"
    }
  }
}
```

or:

```json
{
  "Atlas": {
    "Memory": {
      "StorageMode": "Sqlite"
    }
  },
  "ConnectionStrings": {
    "AtlasMemory": "Data Source=atlas.db"
  }
}
```

The storage selection is interpreted by the memory dependency-injection module.

The result is:

```text
InMemory
    IAtlasMemoryStore
        → InMemoryAtlasMemoryStore

Sqlite
    IAtlasMemoryStore
        → EntityFrameworkAtlasMemoryStore
```

The corresponding initializer is selected at the same time.

This keeps the provider and its initialization mechanism consistent.

## Dependency direction
The dependency direction is intentionally one-way.

```text
Atlas.Hosting
     │
     ├── composes Atlas modules
     │
     ▼
Atlas.Memory
     │
     ├── IAtlasMemory
     ├── IAtlasMemoryStore
     ├── persistence models
     ├── EF Core implementation
     └── initialization
```

Higher-level application capabilities use abstractions rather than depending on storage implementations.

In particular:

```text
Atlas.Interaction
        ↓
    IAtlasMemory

Atlas.Commands
        ↓
    command handlers
        ↓
    IAtlasMemory

Atlas.Memory
        ↓
IAtlasMemoryStore
        ↓
provider implementation
```

Interaction and command code therefore remain independent of SQLite.

### Provider-specific dependency rule
Provider-specific infrastructure stays inside the memory capability.

For example, SQLite and Entity Framework Core references belong to `Atlas.Memory`, not to `Atlas.Interaction` or `Atlas.Commands`.

`Atlas.Hosting` composes the selected implementation but does not become responsible for persistence details.

## Dependency injection
`AddAtlasMemory()` selects the configured memory provider.

The registrations are conceptually:

```text
StorageMode = InMemory
    ↓
IAtlasMemoryStore
    → InMemoryAtlasMemoryStore

IAtlasMemoryStoreInitializer
    → InMemoryAtlasMemoryStoreInitializer
```

and:

```text
StorageMode = Sqlite
    ↓
IAtlasMemoryStore
    → EntityFrameworkAtlasMemoryStore

IAtlasMemoryStoreInitializer
    → EntityFrameworkAtlasMemoryStoreInitializer
```

The application-facing registration remains:

```text
IAtlasMemory
    → AtlasMemory
```

`AtlasMemory` receives the selected `IAtlasMemoryStore` through dependency injection.

This means the rest of the application has no provider-specific branching.

## Test strategy
Persistent-memory integration tests use a dedicated test-support project:

```text
tests/Atlas.Testing
```

The shared infrastructure provides:

```text
PersistentMemoryTestEnvironment
```

Each test environment receives an isolated SQLite database file.

The infrastructure provides:

- a unique temporary database path
- SQLite configuration
- deterministic connection behavior
- deterministic cleanup
- deterministic interaction configuration
- application-builder creation using the real Atlas composition root

Persistent integration tests therefore run without requiring a developer to install or configure a personal database.

### What is tested
The persistent-memory test suite verifies:

```text
Schema initialization
    ✓ fresh SQLite database
    ✓ migrations applied
    ✓ no pending migrations

Persistence
    ✓ Store → dispose → Get
    ✓ Store → dispose → Search

Command pipeline
    ✓ StoreMemoryCommand
    ✓ GetMemoryCommand
    ✓ SearchMemoryCommand

Interaction pipeline
    ✓ natural-language memory storage
    ✓ natural-language memory search

Isolation
    ✓ separate test environments
      cannot see each other's data
```

The integration tests use the normal Atlas dependency-injection composition root rather than constructing the persistent store directly.

This is important because it verifies the architecture as deployed, not merely individual classes in isolation.

## Application selection of persistent storage
The application selects persistent storage through configuration.

The selection flow is:

```text
application configuration
        ↓
Atlas.Hosting
        ↓
AddAtlasMemory(configuration)
        ↓
AtlasMemoryStorageMode
        ↓
selected IAtlasMemoryStore
        ↓
AtlasMemory
        ↓
rest of Atlas application
```

The application therefore has one memory API regardless of the selected provider.

For example:

```text
SQLite configuration
        ↓
EntityFrameworkAtlasMemoryStore
```

while:

```text
InMemory configuration
        ↓
InMemoryAtlasMemoryStore
```

Command and interaction behavior does not change.

## Future storage providers
The architecture is designed so that additional storage providers can be introduced without changing interaction or command behavior.

For example, a future provider might be:

```text
PostgreSQLAtlasMemoryStore
```

or:

```text
CloudAtlasMemoryStore
```

A new provider would implement:

```csharp
IAtlasMemoryStore
```

and, when initialization is required, provide a corresponding:

```csharp
IAtlasMemoryStoreInitializer
```

The dependency-injection module would select the provider based on configuration.

Conceptually:

```text
                    IAtlasMemory
                         │
                         ▼
                    AtlasMemory
                         │
                         ▼
                IAtlasMemoryStore
                         │
          ┌──────────────┼──────────────┐
          ▼              ▼              ▼
      InMemory         SQLite        FutureProvider
```

The following layers would remain unchanged:

```text
Interaction
Commands
Runtime
Application-level memory behavior
```

This is the main architectural benefit of the storage abstraction.

Adding a storage provider should be a composition and infrastructure change, not an application-behavior change.

## Architectural rules
The following rules should remain true as Atlas evolves.

### Rule 1 — application code uses `IAtlasMemory`
Interaction and command code should use the memory capability rather than a storage implementation.

### Rule 2 — storage code uses `IAtlasMemoryStore`
Persistence providers should implement the storage abstraction instead of exposing provider-specific APIs to the application.

### Rule 3 — persistence models stay inside `Atlas.Memory`
Database-specific representations should not leak into interaction or command layers.

### Rule 4 — initialization is separate from operations
Schema creation and migration belong to startup initialization, not to memory commands or interaction handlers.

### Rule 5 — migrations define schema evolution
The persistent database is maintained through EF Core migrations rather than direct schema recreation.

### Rule 6 — provider selection happens through configuration and DI
Application code should not contain provider-specific `if`/`switch` logic for normal memory operations.

### Rule 7 — persistent integration tests use isolated storage
Tests should never depend on a developer's personal or shared database.

## Summary
Persistent memory in Atlas is built around three layers:

```text
Memory capability
    IAtlasMemory
        ↓
    AtlasMemory

Storage abstraction
    IAtlasMemoryStore
        ↓
    provider implementation

Persistence infrastructure
    AtlasMemoryDbContext
    AtlasMemoryRecord
    AtlasMemoryRecordMapper
    EF Core migrations
```

Startup initialization is handled separately through:

```text
IAtlasMemoryStoreInitializer
```

and invoked by `Atlas.Hosting` before the Atlas runtime starts.

This architecture keeps memory behavior independent from persistence technology and allows future storage providers to be added without changing interaction or command behavior.

The intended dependency boundary is:

```text
Commands / Interaction
          ↓
     IAtlasMemory
          ↓
      AtlasMemory
          ↓
   IAtlasMemoryStore
          ↓
   Storage provider
```

As long as that boundary is preserved, Atlas can evolve its persistence strategy without forcing changes throughout the application.
