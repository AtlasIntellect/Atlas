using Atlas.Abstractions.Memory;
using Xunit;

namespace Atlas.Memory.Tests.Memory;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemory"/> class.
/// </summary>
public sealed class AtlasMemoryTests
{
    /// <summary>
    /// Verifies that <see cref="AtlasMemory.StoreAsync"/> stores a memory.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task StoreAsync_Should_StoreMemory()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.GetAsync(
            entry.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(entry, result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.GetAsync"/> returns a previously stored memory.
    /// </summary>
    [Fact]
    public async Task GetAsync_Should_ReturnStoredMemory()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.GetAsync(
            entry.Id,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(entry.Id, result.Id);
        Assert.Equal(entry.Content, result.Content);
        Assert.Equal(entry.CreatedAt, result.CreatedAt);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.GetAsync"/> returns null
    /// when the requested memory does not exist.
    /// </summary>
    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenMemoryDoesNotExist()
    {
        var memory = new AtlasMemory();

        var result = await memory.GetAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that storing another memory with the same identifier replaces
    /// the previously stored memory.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_ReplaceExistingMemoryWithSameId()
    {
        var memory = new AtlasMemory();
        var id = Guid.NewGuid();

        var first = new AtlasMemoryEntry
        {
            Id = id,
            Content = "First memory",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var second = new AtlasMemoryEntry
        {
            Id = id,
            Content = "Updated memory",
            CreatedAt = first.CreatedAt.AddMinutes(1)
        };

        await memory.StoreAsync(
            first,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            second,
            TestContext.Current.CancellationToken);

        var result = await memory.GetAsync(
            id,
            TestContext.Current.CancellationToken);

        Assert.Equal(second, result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.StoreAsync"/> throws when the
    /// cancellation token has already been cancelled.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_Throw_WhenCancellationRequested()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => memory.StoreAsync(
                entry,
                cancellationTokenSource.Token));
    }

    private static AtlasMemoryEntry CreateMemory()
    {
        return new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}