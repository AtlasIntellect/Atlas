using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

/// <summary>
/// Provides unit tests for the <see cref="StoreMemoryCommandHandler"/> class.
/// </summary>
public sealed class StoreMemoryCommandHandlerTests
{
    /// <summary>
    /// Verifies that the handler creates and stores a memory entry
    /// containing the command content.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_CreateAndStoreMemory()
    {
        var memory = new TestMemory();
        var handler = new StoreMemoryCommandHandler(memory);

        var command = new StoreMemoryCommand("Test memory");

        var result = await handler.HandleAsync(
            command,
            TestContext.Current.CancellationToken);

        Assert.Equal("Test memory", result.Content);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotNull(memory.StoredEntry);
        Assert.Equal(result, memory.StoredEntry);
    }

    /// <summary>
    /// Verifies that the handler passes the cancellation token to the memory service.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassCancellationToken_ToMemory()
    {
        var memory = new TestMemory();
        var handler = new StoreMemoryCommandHandler(memory);
        using var cancellationTokenSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new StoreMemoryCommand("Test memory"),
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
            ThrowOnStore = true
        };

        var handler = new StoreMemoryCommandHandler(memory);

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                new StoreMemoryCommand("Test memory"),
                cancellationTokenSource.Token));
    }

    private sealed class TestMemory : IAtlasMemory
    {
        public AtlasMemoryEntry? StoredEntry { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public bool ThrowOnStore { get; init; }

        public Task StoreAsync(
            AtlasMemoryEntry memory,
            CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;

            if (ThrowOnStore)
                throw new OperationCanceledException(cancellationToken);

            StoredEntry = memory;

            return Task.CompletedTask;
        }

        public Task<AtlasMemoryEntry?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                StoredEntry?.Id == id
                    ? StoredEntry
                    : null);
        }

        public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);
        }
    }
}