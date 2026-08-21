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

        var command = new StoreMemoryCommand(
            "Test memory",
            AtlasMemoryType.Fact);

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
            new StoreMemoryCommand(
                "Test memory",
                AtlasMemoryType.Fact),
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
                new StoreMemoryCommand(
                    "Test memory",
                    AtlasMemoryType.Fact),
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that the handler preserves interpreted memory data
    /// in the stored memory entry.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PreserveInterpretationData()
    {
        var memory = new TestMemory();
        var handler = new StoreMemoryCommandHandler(memory);

        var data = new AtlasTaskData
        {
            Description = "Buy milk."
        };

        var command = new StoreMemoryCommand(
            "Buy milk.",
            AtlasMemoryType.Task,
            data);

        var result = await handler.HandleAsync(
            command,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result.Interpretation);
        Assert.Same(
            data,
            result.Interpretation.Data);

        Assert.NotNull(memory.StoredEntry);
        Assert.NotNull(memory.StoredEntry.Interpretation);
        Assert.Same(
            data,
            memory.StoredEntry.Interpretation.Data);
    }

    /// <summary>
    /// Verifies that the handler does not create an interpretation
    /// when the command contains no interpreted data.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_NotCreateInterpretation_WhenDataIsNull()
    {
        var memory = new TestMemory();
        var handler = new StoreMemoryCommandHandler(memory);

        var command = new StoreMemoryCommand(
            "Test memory",
            AtlasMemoryType.Fact);

        var result = await handler.HandleAsync(
            command,
            TestContext.Current.CancellationToken);

        Assert.Null(result.Interpretation);
        Assert.NotNull(memory.StoredEntry);
        Assert.Null(memory.StoredEntry.Interpretation);
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