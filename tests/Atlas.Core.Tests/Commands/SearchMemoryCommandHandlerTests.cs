using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

/// <summary>
/// Provides unit tests for the <see cref="SearchMemoryCommandHandler"/> class.
/// </summary>
public sealed class SearchMemoryCommandHandlerTests
{
    /// <summary>
    /// Verifies that the handler returns matching memories.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnMatchingMemories()
    {
        var entries = new List<AtlasMemoryEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Content = "First matching memory.",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Content = "Second matching memory.",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        var memory = new TestMemory
        {
            Entries = entries
        };

        var handler = new SearchMemoryCommandHandler(memory);

        var result = await handler.HandleAsync(
            new SearchMemoryCommand("matching"),
            TestContext.Current.CancellationToken);
        
        Assert.Equal(entries, result);
    }

    /// <summary>
    /// Verifies that the handler returns an empty collection when no memories match.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnEmpty_WhenNoMemoriesMatch()
    {
        var memory = new TestMemory();

        var handler = new SearchMemoryCommandHandler(memory);

        var result = await handler.HandleAsync(
            new SearchMemoryCommand("missing"),
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that the handler passes the query to the memory service.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassQuery_ToMemory()
    {
        var memory = new TestMemory();
        var handler = new SearchMemoryCommandHandler(memory);

        const string query = "Canon camera";

        await handler.HandleAsync(
            new SearchMemoryCommand(query),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            query,
            memory.ReceivedQuery);
    }

    /// <summary>
    /// Verifies that the handler passes the cancellation token to the memory service.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassCancellationToken_ToMemory()
    {
        var memory = new TestMemory();
        var handler = new SearchMemoryCommandHandler(memory);

        using var cancellationTokenSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new SearchMemoryCommand("test"),
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            memory.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that the handler propagates cancellation from the memory service.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PropagateCancellation()
    {
        var memory = new TestMemory
        {
            ThrowOnSearch = true
        };

        var handler = new SearchMemoryCommandHandler(memory);

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                new SearchMemoryCommand("test"),
                cancellationTokenSource.Token));
    }

    private sealed class TestMemory : IAtlasMemory
    {
        public IReadOnlyList<AtlasMemoryEntry> Entries { get; init; } = [];

        public string? ReceivedQuery { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public bool ThrowOnSearch { get; init; }

        public Task StoreAsync(
            AtlasMemoryEntry memory,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<AtlasMemoryEntry?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AtlasMemoryEntry?>(null);
        }

        public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            ReceivedQuery = query;
            ReceivedCancellationToken = cancellationToken;

            return ThrowOnSearch ?
                throw new OperationCanceledException(cancellationToken)
                : Task.FromResult(Entries);
        }
    }
}