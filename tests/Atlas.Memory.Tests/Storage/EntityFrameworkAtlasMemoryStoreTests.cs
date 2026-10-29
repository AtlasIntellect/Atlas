using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Atlas.Memory.Storage;
using Atlas.Testing.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Atlas.Memory.Tests.Storage;

/// <summary>
/// Provides integration tests for <see cref="EntityFrameworkAtlasMemoryStore"/>.
/// </summary>
public sealed class EntityFrameworkAtlasMemoryStoreTests
{
    /// <summary>
    /// Verifies that a memory can be stored and retrieved.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_PersistMemory()
    {
        await using var database = CreateDatabase();

        var memory = CreateMemory();

        await database.Store.StoreAsync(
            memory,
            TestContext.Current.CancellationToken);

        var result =
            await database.Store.GetAsync(
                memory.Id,
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(
            memory.Id,
            result.Id);

        Assert.Equal(
            memory.Content,
            result.Content);

        Assert.Equal(
            memory.CreatedAt,
            result.CreatedAt);

        Assert.Equal(
            memory.Type,
            result.Type);
    }

    /// <summary>
    /// Verifies that storing a memory with an existing identifier updates the persisted record.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_UpdateExistingMemory()
    {
        await using var database = CreateDatabase();

        var id = Guid.NewGuid();

        var original =
            CreateMemory(
                id,
                "Original content.");

        var updated =
            CreateMemory(
                id,
                "Updated content.");

        await database.Store.StoreAsync(
            original,
            TestContext.Current.CancellationToken);

        await database.Store.StoreAsync(
            updated,
            TestContext.Current.CancellationToken);

        var result =
            await database.Store.GetAsync(
                id,
                TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal(
            "Updated content.",
            result.Content);
    }

    /// <summary>
    /// Verifies that retrieving an unknown identifier returns null.
    /// </summary>
    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenMemoryDoesNotExist()
    {
        await using var database = CreateDatabase();

        var result =
            await database.Store.GetAsync(
                Guid.NewGuid(),
                TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that memories matching all search terms are returned.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnMatchingMemories()
    {
        await using var database = CreateDatabase();

        await database.Store.StoreAsync(
            CreateMemory(
                Guid.NewGuid(),
                "I bought a Canon camera."),
            TestContext.Current.CancellationToken);

        await database.Store.StoreAsync(
            CreateMemory(
                Guid.NewGuid(),
                "My camera has an 18-55mm lens."),
            TestContext.Current.CancellationToken);

        await database.Store.StoreAsync(
            CreateMemory(
                Guid.NewGuid(),
                "I like photography."),
            TestContext.Current.CancellationToken);

        var results =
            await database.Store.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "camera"
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(2, results.Count);

        Assert.All(
            results,
            memory =>
                Assert.Contains(
                    "camera",
                    memory.Content,
                    StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that search requires all query terms to match.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_RequireAllTermsToMatch()
    {
        await using var database = CreateDatabase();

        var matchingMemory =
            CreateMemory(
                Guid.NewGuid(),
                "I bought a Canon camera.");

        var partialMatch =
            CreateMemory(
                Guid.NewGuid(),
                "I bought a Canon lens.");

        await database.Store.StoreAsync(
            matchingMemory,
            TestContext.Current.CancellationToken);

        await database.Store.StoreAsync(
            partialMatch,
            TestContext.Current.CancellationToken);

        var results =
            await database.Store.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "Canon camera"
                },
                TestContext.Current.CancellationToken);

        var result = Assert.Single(results);

        Assert.Equal(
            matchingMemory.Id,
            result.Id);
    }

    /// <summary>
    /// Verifies that search results can be filtered by memory type.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_FilterByMemoryType()
    {
        await using var database = CreateDatabase();

        await database.Store.StoreAsync(
            CreateMemory(
                Guid.NewGuid(),
                "I bought a Canon camera."),
            TestContext.Current.CancellationToken);

        await database.Store.StoreAsync(
            CreateMemory(
                Guid.NewGuid(),
                "My favorite camera is Canon.",
                AtlasMemoryType.Preference),
            TestContext.Current.CancellationToken);

        var results =
            await database.Store.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "Canon",
                    Type = AtlasMemoryType.Preference
                },
                TestContext.Current.CancellationToken);

        var result = Assert.Single(results);

        Assert.Equal(
            AtlasMemoryType.Preference,
            result.Type);
    }

    /// <summary>
    /// Verifies that an empty search returns memories ordered by creation date.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnNewestMemories_WhenQueryIsEmpty()
    {
        await using var database = CreateDatabase();

        var older =
            CreateMemory(
                Guid.NewGuid(),
                "Older memory.",
                createdAt:
                    new DateTimeOffset(
                        2026,
                        9,
                        1,
                        10,
                        0,
                        0,
                        TimeSpan.Zero));

        var newer =
            CreateMemory(
                Guid.NewGuid(),
                "Newer memory.",
                createdAt:
                    new DateTimeOffset(
                        2026,
                        9,
                        2,
                        10,
                        0,
                        0,
                        TimeSpan.Zero));

        await database.Store.StoreAsync(
            older,
            TestContext.Current.CancellationToken);

        await database.Store.StoreAsync(
            newer,
            TestContext.Current.CancellationToken);

        var results =
            await database.Store.SearchAsync(
                new AtlasMemoryQuery(),
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [newer.Id, older.Id],
            results.Select(
                memory => memory.Id));
    }

    /// <summary>
    /// Verifies that cancellation is honored when storing a memory.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_Throw_WhenCancellationRequested()
    {
        await using var database = CreateDatabase();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => database.Store.StoreAsync(
                CreateMemory(),
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that cancellation is honored when retrieving a memory.
    /// </summary>
    [Fact]
    public async Task GetAsync_Should_Throw_WhenCancellationRequested()
    {
        await using var database = CreateDatabase();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => database.Store.GetAsync(
                Guid.NewGuid(),
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that cancellation is honored when searching memories.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Throw_WhenCancellationRequested()
    {
        await using var database = CreateDatabase();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => database.Store.SearchAsync(
                new AtlasMemoryQuery
                {
                    Text = "camera"
                },
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that updating an existing memory preserves the original
    /// CreatedAt timestamp and updates the UpdatedAt timestamp.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Memory_And_Preserve_CreatedAt()
    {
        // Arrange
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        await host.StartAsync(TestContext.Current.CancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Original content",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Type = AtlasMemoryType.Fact
        };

        await store.StoreAsync(original, TestContext.Current.CancellationToken);

        var updated = new AtlasMemoryEntry
        {
            Id = original.Id,
            Content = "Updated content",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Task,
            LifecycleState = AtlasMemoryLifecycleState.Archived
        };

        var updateStartedAt = DateTimeOffset.UtcNow;

        // Act
        await store.UpdateAsync(updated, TestContext.Current.CancellationToken);

        var result =
            await store.GetAsync(original.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal("Updated content", result.Content);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
        Assert.Equal(AtlasMemoryType.Task, result.Type);
        Assert.Equal(
            AtlasMemoryLifecycleState.Archived,
            result.LifecycleState);

        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt.Value >= updateStartedAt);
    }

    /// <summary>
    /// Verifies that updating a memory that does not exist
    /// throws a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Memory_Does_Not_Exist()
    {
        // Arrange
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        await host.StartAsync(TestContext.Current.CancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Updated content",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => store.UpdateAsync(memory, TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains(memory.Id.ToString(), exception.Message);
    }

    /// <summary>
    /// Verifies that cancellation is honored when updating a memory.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Honor_Cancellation()
    {
        // Arrange
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        await host.StartAsync(TestContext.Current.CancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Original content",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        await store.StoreAsync(original, TestContext.Current.CancellationToken);

        var updated = new AtlasMemoryEntry
        {
            Id = original.Id,
            Content = "Updated content",
            CreatedAt = original.CreatedAt
        };

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.UpdateAsync(updated, cancellation.Token));

        // Assert
        var result =
            await store.GetAsync(original.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Original content", result.Content);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
    }

    /// <summary>
    /// Verifies that updating a memory with a new interpretation replaces
    /// the existing interpretation.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Replace_Existing_Interpretation_When_Supplied()
    {
        // Arrange
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        await host.StartAsync(TestContext.Current.CancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Call the dentist",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Interpretation = new AtlasMemoryInterpretation
            {
                Data = new AtlasTaskData
                {
                    Description = "Call the dentist"
                }
            }
        };

        await store.StoreAsync(original, TestContext.Current.CancellationToken);

        var updated = new AtlasMemoryEntry
        {
            Id = original.Id,
            Content = "Call the veterinarian",
            CreatedAt = original.CreatedAt,
            Interpretation = new AtlasMemoryInterpretation
            {
                Data = new AtlasTaskData
                {
                    Description = "Call the veterinarian"
                }
            }
        };

        // Act
        await store.UpdateAsync(updated, TestContext.Current.CancellationToken);

        var result =
            await store.GetAsync(original.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Interpretation);

        var taskData = Assert.IsType<AtlasTaskData>(
            result.Interpretation.Data);

        Assert.Equal("Call the veterinarian", taskData.Description);
    }

    /// <summary>
    /// Verifies that archiving a memory preserves its content, creation time,
    /// type, interpretation, confidence, and provenance while updating its
    /// lifecycle state and updated timestamp.
    /// </summary>
    [Fact]
    public async Task ArchiveAsync_Should_Preserve_Memory_And_Interpretation()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var provenance = new AtlasMemoryProvenance
        {
            SourceType = AtlasMemorySourceType.Conversation,
            SourceId = "conversation-123",
            Description = "Explicit user statement",
            Metadata = new Dictionary<string, string>
            {
                ["method"] = "explicit-statement"
            }
        };

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Call the dentist",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Type = AtlasMemoryType.Task,
            Confidence = 0.91,
            Provenance = provenance,
            Interpretation = new AtlasMemoryInterpretation
            {
                Data = new AtlasTaskData
                {
                    Description = "Call the dentist"
                }
            }
        };

        await store.StoreAsync(original, cancellationToken);
        var archiveStartedAt = DateTimeOffset.UtcNow;

        await store.ArchiveAsync(original.Id, cancellationToken);

        var result = await store.GetAsync(original.Id, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Content, result.Content);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
        Assert.Equal(original.Type, result.Type);
        Assert.Equal(
            AtlasMemoryLifecycleState.Archived,
            result.LifecycleState);

        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt.Value >= archiveStartedAt);

        Assert.NotNull(result.Interpretation);

        var taskData = Assert.IsType<AtlasTaskData>(
            result.Interpretation.Data);

        Assert.Equal("Call the dentist", taskData.Description);

        Assert.Equal(0.91, result.Confidence);

        Assert.NotNull(result.Provenance);
        Assert.Equal(provenance.SourceType, result.Provenance.SourceType);
        Assert.Equal(provenance.SourceId, result.Provenance.SourceId);
        Assert.Equal(provenance.Description, result.Provenance.Description);
        Assert.Equal(
            "explicit-statement",
            result.Provenance.Metadata["method"]);
    }

    /// <summary>
    /// Verifies that ArchiveAsync throws a <exception cref="KeyNotFoundException"></exception>
    /// when attempting to archive a non-existent memory.
    /// </summary>
    [Fact]
    public async Task ArchiveAsync_Should_Throw_When_Memory_Does_Not_Exist()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();
        var id = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => store.ArchiveAsync(id, cancellationToken));

        Assert.Contains(id.ToString(), exception.Message);
    }

    /// <summary>
    /// Verifies that deleting a memory entry using <see cref="IAtlasMemoryStore.DeleteAsync(Guid, CancellationToken)"/>
    /// removes it permanently from the memory store.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_Permanently_Remove_Memory()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Memory to delete",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await store.StoreAsync(memory, cancellationToken);
        await store.DeleteAsync(memory.Id, cancellationToken);

        Assert.Null(await store.GetAsync(memory.Id, cancellationToken));

        var results = await store.SearchAsync(
            new AtlasMemoryQuery { IncludeArchived = true },
            cancellationToken);

        Assert.DoesNotContain(results, entry => entry.Id == memory.Id);
    }

    /// <summary>
    /// Verifies that deleting an item asynchronously multiple times does not result in an error and the item remains
    /// deleted.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_Be_Idempotent()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();
        var id = Guid.NewGuid();

        await store.DeleteAsync(id, cancellationToken);
        await store.DeleteAsync(id, cancellationToken);

        Assert.Null(await store.GetAsync(id, cancellationToken));
    }

    /// <summary>
    /// Verifies that archived memories are excluded by default when searching asynchronously.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Exclude_Archived_Memories_By_Default()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var active = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Dentist appointment",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var archived = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Dentist appointment history",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        await store.StoreAsync(active, cancellationToken);
        await store.StoreAsync(archived, cancellationToken);
        await store.ArchiveAsync(archived.Id, cancellationToken);

        var results = await store.SearchAsync(
            new AtlasMemoryQuery { Text = "Dentist" },
            cancellationToken);

        Assert.Contains(results, entry => entry.Id == active.Id);
        Assert.DoesNotContain(results, entry => entry.Id == archived.Id);
    }

    /// <summary>
    /// Verifies that archived memories are included in search results when requested.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Include_Archived_Memories_When_Requested()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var archived = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Dentist appointment history",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await store.StoreAsync(archived, cancellationToken);
        await store.ArchiveAsync(archived.Id, cancellationToken);

        var results = await store.SearchAsync(
            new AtlasMemoryQuery
            {
                Text = "Dentist",
                IncludeArchived = true
            },
            cancellationToken);

        Assert.Contains(results, entry => entry.Id == archived.Id);
    }

    /// <summary>
    /// Verifies that the <see cref="IAtlasMemoryStore.UpdateAsync"/> method preserves the 
    /// <see cref="AtlasMemoryEntry.Confidence"/> and <see cref="AtlasMemoryEntry.Provenance"/> properties 
    /// of an existing memory entry when these properties are not supplied in the updated entry.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_PreserveConfidenceAndProvenance_When_Not_Supplied()
    {
        using var environment = new PersistentMemoryTestEnvironment();
        using var host = environment.CreateBuilder().Build();

        var cancellationToken = TestContext.Current.CancellationToken;
        await host.StartAsync(cancellationToken);

        var store = host.Services.GetRequiredService<IAtlasMemoryStore>();

        var provenance = new AtlasMemoryProvenance
        {
            SourceType = AtlasMemorySourceType.Conversation,
            SourceId = "conversation-123",
            Description = "Explicit user statement",
            Metadata = new Dictionary<string, string>
            {
                ["method"] = "explicit-statement"
            }
        };

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Original content",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Confidence = 0.91,
            Provenance = provenance
        };

        await store.StoreAsync(original, cancellationToken);

        var updated = new AtlasMemoryEntry
        {
            Id = original.Id,
            Content = "Updated content",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        await store.UpdateAsync(updated, cancellationToken);

        var result = await store.GetAsync(
            original.Id,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated content", result.Content);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
        Assert.Equal(0.91, result.Confidence);

        Assert.NotNull(result.Provenance);
        Assert.Equal(provenance.SourceType, result.Provenance.SourceType);
        Assert.Equal(provenance.SourceId, result.Provenance.SourceId);
        Assert.Equal(provenance.Description, result.Provenance.Description);
        Assert.Equal(
            "explicit-statement",
            result.Provenance.Metadata["method"]);
    }

    private static TestDatabase CreateDatabase()
    {
        var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        connection.Open();

        var services = new ServiceCollection();

        services.AddDbContextFactory<AtlasMemoryDbContext>(
            options => options.UseSqlite(connection));

        var provider = services.BuildServiceProvider();

        var factory =
            provider.GetRequiredService<
                IDbContextFactory<AtlasMemoryDbContext>>();

        using (var context = factory.CreateDbContext())
        {
            context.Database.EnsureCreated();
        }

        var store =
            new EntityFrameworkAtlasMemoryStore(
                factory);

        return new TestDatabase(
            connection,
            provider,
            store);
    }

    private static AtlasMemoryEntry CreateMemory(
        Guid? id = null,
        string content = "Test memory.",
        AtlasMemoryType type = AtlasMemoryType.Fact,
        DateTimeOffset? createdAt = null)
    {
        return new AtlasMemoryEntry
        {
            Id = id ?? Guid.NewGuid(),
            Content = content,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            Type = type
        };
    }

    private sealed class TestDatabase(
        SqliteConnection connection,
        ServiceProvider provider,
        EntityFrameworkAtlasMemoryStore store)
        : IAsyncDisposable
    {
        public EntityFrameworkAtlasMemoryStore Store =>
            store;

        public async ValueTask DisposeAsync()
        {
            await provider.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
