using Atlas.Commands.Interfaces;
using Atlas.Memory.Commands;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Atlas.Testing.Persistence;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Atlas.Hosting.Tests.Memory;

/// <summary>
/// Provides tests for the persistent-memory test infrastructure.
/// </summary>
public sealed class PersistentMemoryTestEnvironmentTests
{
    /// <summary>
    /// Verifies that separate persistent-memory test environments
    /// use isolated databases.
    /// </summary>
    [Fact]
    public async Task Environments_Should_IsolatePersistentMemory()
    {
        using var firstEnvironment =
            new PersistentMemoryTestEnvironment();

        using var secondEnvironment =
            new PersistentMemoryTestEnvironment();

        var memoryEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Memory from first test environment",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        using var firstHost =
            firstEnvironment.CreateBuilder().Build();

        await firstHost.StartAsync(
            TestContext.Current.CancellationToken);

        var firstMemory =
            firstHost.Services.GetRequiredService<IAtlasMemory>();

        await firstMemory.StoreAsync(
            memoryEntry,
            TestContext.Current.CancellationToken);

        await firstHost.StopAsync(
            TestContext.Current.CancellationToken);

        using var secondHost =
            secondEnvironment.CreateBuilder().Build();

        await secondHost.StartAsync(
            TestContext.Current.CancellationToken);

        var secondMemory =
            secondHost.Services.GetRequiredService<IAtlasMemory>();

        var result =
            await secondMemory.GetAsync(
                memoryEntry.Id,
                TestContext.Current.CancellationToken);

        Assert.Null(result);

        await secondHost.StopAsync(
            TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that each persistent-memory test environment
    /// receives a unique database path.
    /// </summary>
    [Fact]
    public void Environments_Should_UseUniqueDatabasePaths()
    {
        using var firstEnvironment =
            new PersistentMemoryTestEnvironment();

        using var secondEnvironment =
            new PersistentMemoryTestEnvironment();

        Assert.NotEqual(
            firstEnvironment.DatabasePath,
            secondEnvironment.DatabasePath);
    }

    /// <summary>
    /// Verifies that a memory update persists across application recreation
    /// when using the same persistent-memory test environment.
    /// </summary>
    [Fact]
    public async Task UpdateMemoryCommand_Should_Persist_Across_Application_Recreation()
    {
        // Arrange
        using var environment = new PersistentMemoryTestEnvironment();

        var cancellationToken = TestContext.Current.CancellationToken;

        Guid memoryId;
        DateTimeOffset createdAt;
        DateTimeOffset updatedAt;

        // First application: store and update the memory through the dispatcher.
        using (var firstHost = environment.CreateBuilder().Build())
        {
            await firstHost.StartAsync(cancellationToken);

            var dispatcher = firstHost.Services
                .GetRequiredService<IAtlasCommandDispatcher>();

            var original = await dispatcher.DispatchAsync<
                StoreMemoryCommand,
                AtlasMemoryEntry>(
                new StoreMemoryCommand(
                    "Call the dentist",
                    AtlasMemoryType.Task,
                    new AtlasTaskData
                    {
                        Description = "Call the dentist"
                    }),
                cancellationToken);

            var updated = await dispatcher.DispatchAsync<
                UpdateMemoryCommand,
                AtlasMemoryEntry>(
                new UpdateMemoryCommand(
                    original.Id,
                    "Call the dentist tomorrow",
                    AtlasMemoryType.Task),
                cancellationToken);

            // Assert the update result.
            Assert.Equal(original.Id, updated.Id);
            Assert.Equal(
                "Call the dentist tomorrow",
                updated.Content);
            Assert.Equal(original.CreatedAt, updated.CreatedAt);
            Assert.NotNull(updated.UpdatedAt);

            Assert.NotNull(updated.Interpretation);

            var updatedTaskData = Assert.IsType<AtlasTaskData>(
                updated.Interpretation.Data);

            Assert.Equal("Call the dentist", updatedTaskData.Description);

            memoryId = updated.Id;
            createdAt = updated.CreatedAt;
            updatedAt = updated.UpdatedAt.Value;

            await firstHost.StopAsync(cancellationToken);
        }

        // Second application: read the memory from the same SQLite database.
        using (var secondHost = environment.CreateBuilder().Build())
        {
            await secondHost.StartAsync(cancellationToken);

            var memory = secondHost.Services
                .GetRequiredService<IAtlasMemory>();

            var persisted = await memory.GetAsync(
                memoryId,
                cancellationToken);

            // Assert the update survived application recreation.
            Assert.NotNull(persisted);
            Assert.Equal(memoryId, persisted.Id);
            Assert.Equal(
                "Call the dentist tomorrow",
                persisted.Content);
            Assert.Equal(createdAt, persisted.CreatedAt);
            Assert.Equal(updatedAt, persisted.UpdatedAt);
            Assert.Equal(AtlasMemoryType.Task, persisted.Type);

            Assert.NotNull(persisted.Interpretation);

            var persistedTaskData = Assert.IsType<AtlasTaskData>(
                persisted.Interpretation.Data);

            Assert.Equal(
                "Call the dentist",
                persistedTaskData.Description);

            await secondHost.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Verifies that a memory archive persists across application recreation
    /// when using the same persistent-memory test environment.
    /// </summary>
    [Fact]
    public async Task ArchiveMemoryCommand_Should_Persist_Across_Application_Recreation()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        var cancellationToken = TestContext.Current.CancellationToken;

        Guid memoryId;
        DateTimeOffset createdAt;

        // First application: store and archive through the command dispatcher.
        using (var firstHost = environment.CreateBuilder().Build())
        {
            await firstHost.StartAsync(cancellationToken);

            var dispatcher = firstHost.Services
                .GetRequiredService<IAtlasCommandDispatcher>();

            var original = await dispatcher.DispatchAsync<
                StoreMemoryCommand,
                AtlasMemoryEntry>(
                new StoreMemoryCommand(
                    "Dentist appointment history",
                    AtlasMemoryType.Task,
                    new AtlasTaskData
                    {
                        Description = "Call the dentist"
                    }),
                cancellationToken);

            var archived = await dispatcher.DispatchAsync<
                ArchiveMemoryCommand,
                AtlasMemoryEntry>(
                new ArchiveMemoryCommand(original.Id),
                cancellationToken);

            Assert.Equal(original.Id, archived.Id);
            Assert.Equal(original.CreatedAt, archived.CreatedAt);
            Assert.Equal(
                AtlasMemoryLifecycleState.Archived,
                archived.LifecycleState);
            Assert.NotNull(archived.UpdatedAt);

            memoryId = archived.Id;
            createdAt = archived.CreatedAt;

            await firstHost.StopAsync(cancellationToken);
        }

        // Second application: verify persistence and search behavior.
        using (var secondHost = environment.CreateBuilder().Build())
        {
            await secondHost.StartAsync(cancellationToken);

            var memory = secondHost.Services
                .GetRequiredService<IAtlasMemory>();

            var persisted = await memory.GetAsync(
                memoryId,
                cancellationToken);

            Assert.NotNull(persisted);
            Assert.Equal(memoryId, persisted.Id);
            Assert.Equal(createdAt, persisted.CreatedAt);
            Assert.Equal(
                AtlasMemoryLifecycleState.Archived,
                persisted.LifecycleState);
            Assert.NotNull(persisted.UpdatedAt);

            // Archived memories are excluded from normal search.
            var activeResults = await memory.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "dentist"
                },
                cancellationToken);

            Assert.DoesNotContain(
                activeResults,
                entry => entry.Id == memoryId);

            // Explicitly requesting archived memories finds the entry.
            var archivedResults = await memory.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "dentist",
                    IncludeArchived = true
                },
                cancellationToken);

            Assert.Contains(
                archivedResults,
                entry => entry.Id == memoryId);

            await secondHost.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Verifies that a memory deletion persists across application recreation
    /// when using the same persistent-memory test environment.
    /// </summary>
    [Fact]
    public async Task DeleteMemoryCommand_Should_Persist_Across_Application_Recreation()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        var cancellationToken = TestContext.Current.CancellationToken;

        Guid memoryId;

        // First application: store and permanently delete through the dispatcher.
        using (var firstHost = environment.CreateBuilder().Build())
        {
            await firstHost.StartAsync(cancellationToken);

            var dispatcher = firstHost.Services
                .GetRequiredService<IAtlasCommandDispatcher>();

            var original = await dispatcher.DispatchAsync<
                StoreMemoryCommand,
                AtlasMemoryEntry>(
                new StoreMemoryCommand(
                    "Memory to permanently delete",
                    AtlasMemoryType.Fact),
                cancellationToken);

            memoryId = original.Id;

            var deleted = await dispatcher.DispatchAsync<
                DeleteMemoryCommand,
                bool>(
                new DeleteMemoryCommand(memoryId),
                cancellationToken);

            Assert.True(deleted);

            // Deleting the same ID again is deterministic and harmless.
            var deletedAgain = await dispatcher.DispatchAsync<
                DeleteMemoryCommand,
                bool>(
                new DeleteMemoryCommand(memoryId),
                cancellationToken);

            Assert.True(deletedAgain);

            await firstHost.StopAsync(cancellationToken);
        }

        // Second application: prove the record is still gone.
        using (var secondHost = environment.CreateBuilder().Build())
        {
            await secondHost.StartAsync(cancellationToken);

            var memory = secondHost.Services
                .GetRequiredService<IAtlasMemory>();

            var persisted = await memory.GetAsync(
                memoryId,
                cancellationToken);

            Assert.Null(persisted);

            // Even explicitly including archived memories cannot find a deleted entry.
            var results = await memory.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "permanently delete",
                    IncludeArchived = true
                },
                cancellationToken);

            Assert.DoesNotContain(
                results,
                entry => entry.Id == memoryId);

            await secondHost.StopAsync(cancellationToken);
        }
    }
}
