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

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> returns memories
    /// containing the specified query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnMatchingMemories()
    {
        var memory = new AtlasMemory();

        var matchingEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var nonMatchingEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I like pizza",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            matchingEntry,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            nonMatchingEntry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "Canon",
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(result);
        Assert.Equal(matchingEntry, entry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> performs a
    /// case-insensitive search.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_BeCaseInsensitive()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "CANON",
            TestContext.Current.CancellationToken);

        var matchedEntry = Assert.Single(result);
        Assert.Equal(entry, matchedEntry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> supports partial
    /// matches within memory content.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnPartialMatches()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "cam",
            TestContext.Current.CancellationToken);

        var matchedEntry = Assert.Single(result);
        Assert.Equal(entry, matchedEntry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> returns an empty
    /// collection when no memories match the query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnEmpty_WhenNoMemoriesMatch()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "nonexistent",
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> returns an empty
    /// collection when the query is empty or whitespace.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task SearchAsync_Should_ReturnEmpty_WhenQueryIsEmpty(
        string query)
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            query,
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> throws when the
    /// cancellation token has already been cancelled.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Throw_WhenCancellationRequested()
    {
        var memory = new AtlasMemory();

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => memory.SearchAsync(
                "test",
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> matches memories
    /// containing all terms in a multi-word query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_MatchAllTermsInQuery()
    {
        var memory = new AtlasMemory();

        var matchingEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera recently",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var partialMatchEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            matchingEntry,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            partialMatchEntry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "camera recently",
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(result);

        Assert.Equal(
            matchingEntry,
            entry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> ignores repeated
    /// whitespace when processing a multi-word query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_IgnoreRepeatedWhitespaceInQuery()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera recently",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "camera    recently",
            TestContext.Current.CancellationToken);

        var matchedEntry = Assert.Single(result);

        Assert.Equal(
            entry,
            matchedEntry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync"/> returns matching
    /// memories with the most recently created memory first.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnNewestMemoriesFirst()
    {
        var memory = new AtlasMemory();

        var olderEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

        var newerEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I still have my Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            olderEntry,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            newerEntry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "Canon camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [newerEntry, olderEntry],
            result);
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