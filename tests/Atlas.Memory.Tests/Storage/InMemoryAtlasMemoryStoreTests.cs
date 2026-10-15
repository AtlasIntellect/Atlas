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
}
