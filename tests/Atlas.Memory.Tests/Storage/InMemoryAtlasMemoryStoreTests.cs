using Atlas.Memory.Models;
using Atlas.Memory.Storage;
using Xunit;

namespace Atlas.Memory.Tests.Storage;

/// <summary>
/// Contains unit tests for the <see cref="InMemoryAtlasMemoryStore"/> class.
/// </summary>
public sealed class InMemoryAtlasMemoryStoreTests
{
    /// <summary>
    /// Tests that the <see cref="InMemoryAtlasMemoryStore.UpdateAsync"/> method correctly
    /// updates an existing memory entry while preserving its original CreatedAt timestamp.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Memory_And_Preserve_CreatedAt()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();

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
    /// Tests that the <see cref="InMemoryAtlasMemoryStore.UpdateAsync"/> method throws a
    /// <see cref="KeyNotFoundException"/> when attempting to update a memory entry that does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Memory_Does_Not_Exist()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();

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
    /// Tests that the <see cref="InMemoryAtlasMemoryStore.UpdateAsync"/> method honors cancellation requests.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Honor_Cancellation()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();

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
    /// Tests that the <see cref="InMemoryAtlasMemoryStore.UpdateAsync"/> method
    /// preserves the existing interpretation of a memory entry when
    /// the updated entry does not supply a new interpretation.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Preserve_Interpretation_When_Not_Supplied()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Remember to call the dentist",
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
        var stored = await store.GetAsync(original.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(stored);
        Assert.NotNull(stored.Interpretation);

        var updated = new AtlasMemoryEntry
        {
            Id = original.Id,
            Content = "Remember to call the dentist tomorrow",
            CreatedAt = DateTimeOffset.UtcNow,
            Interpretation = null
        };

        // Act
        await store.UpdateAsync(updated, TestContext.Current.CancellationToken);

        var result = await store.GetAsync(original.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "Remember to call the dentist tomorrow",
            result.Content);

        Assert.NotNull(result.Interpretation);

        var taskData = Assert.IsType<AtlasTaskData>(
            result.Interpretation.Data);

        Assert.Equal("Call the dentist", taskData.Description);
    }

    /// <summary>
    /// Tests that the <see cref="InMemoryAtlasMemoryStore.UpdateAsync"/> method
    /// replaces the existing interpretation of a memory entry when
    /// the updated entry supplies a new interpretation.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Replace_Existing_Interpretation_When_Supplied()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();

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

        var result = await store.GetAsync(
            original.Id,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Interpretation);

        var taskData = Assert.IsType<AtlasTaskData>(
            result.Interpretation.Data);

        Assert.Equal("Call the veterinarian", taskData.Description);
    }

    /// <summary>
    /// Verifies that archiving an existing memory entry updates
    /// its lifecycle state to archived and preserves its data.
    /// </summary>
    [Fact]
    public async Task ArchiveAsync_Should_Archive_Existing_Memory_And_Preserve_Data()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryAtlasMemoryStore();

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Call the dentist",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Type = AtlasMemoryType.Task,
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

        // Act
        await store.ArchiveAsync(original.Id, cancellationToken);

        var result = await store.GetAsync(original.Id, cancellationToken);

        // Assert
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
    }

    /// <summary>
    /// Verifies that ArchiveAsync throws a <exception cref="KeyNotFoundException"></exception>
    /// when attempting to archive a non-existent memory.
    /// </summary>
    [Fact]
    public async Task ArchiveAsync_Should_Throw_When_Memory_Does_Not_Exist()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();
        var id = Guid.NewGuid();

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => store.ArchiveAsync(
                id,
                TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains(id.ToString(), exception.Message);
    }

    /// <summary>
    /// Verifies that deleting a memory entry using <see cref="InMemoryAtlasMemoryStore.DeleteAsync(Guid, CancellationToken)"/>
    /// permanently removes it from the memory store.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_Permanently_Remove_Memory()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryAtlasMemoryStore();

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Memory to delete",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await store.StoreAsync(memory, cancellationToken);

        // Act
        await store.DeleteAsync(memory.Id, cancellationToken);

        // Assert
        var result = await store.GetAsync(memory.Id, cancellationToken);
        Assert.Null(result);

        var searchResults = await store.SearchAsync(
            new AtlasMemoryQuery(),
            cancellationToken);

        Assert.DoesNotContain(
            searchResults,
            entry => entry.Id == memory.Id);
    }

    /// <summary>
    /// Verifies that deleting a non-existent memory entry is idempotent and does not throw or affect state.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_Be_Idempotent_When_Memory_Does_Not_Exist()
    {
        // Arrange
        var store = new InMemoryAtlasMemoryStore();
        var id = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act & Assert
        await store.DeleteAsync(id, cancellationToken);
        await store.DeleteAsync(id, cancellationToken);

        Assert.Null(await store.GetAsync(id, cancellationToken));
    }

    /// <summary>
    /// Verifies that archived memories are excluded by default when searching the memory store.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Exclude_Archived_Memories_By_Default()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryAtlasMemoryStore();

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

        // Act
        var results = await store.SearchAsync(
            new AtlasMemoryQuery { Text = "Dentist" },
            cancellationToken);

        // Assert
        Assert.Contains(results, entry => entry.Id == active.Id);
        Assert.DoesNotContain(results, entry => entry.Id == archived.Id);
    }

    /// <summary>
    /// Verifies that archived memories are included in search results when requested.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Include_Archived_Memories_When_Requested()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryAtlasMemoryStore();

        var archived = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Dentist appointment history",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await store.StoreAsync(archived, cancellationToken);
        await store.ArchiveAsync(archived.Id, cancellationToken);

        // Act
        var results = await store.SearchAsync(
            new AtlasMemoryQuery
            {
                Text = "Dentist",
                IncludeArchived = true
            },
            cancellationToken);

        // Assert
        Assert.Contains(results, entry => entry.Id == archived.Id);
    }
}
