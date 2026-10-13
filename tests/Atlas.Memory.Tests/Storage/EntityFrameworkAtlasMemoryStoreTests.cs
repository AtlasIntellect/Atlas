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
