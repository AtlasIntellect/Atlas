using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

/// <summary>
/// Provides unit tests for the <see cref="GetMemoryCommandHandler"/> class.
/// </summary>
public sealed class GetMemoryCommandHandlerTests
{
    /// <summary>
    /// Verifies that the handler returns the requested memory entry.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnMemory()
    {
        var memoryId = Guid.NewGuid();

        var entry = new AtlasMemoryEntry
        {
            Id = memoryId,
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var memory = new TestMemory
        {
            Entry = entry
        };

        var handler = new GetMemoryCommandHandler(memory);

        var result = await handler.HandleAsync(
            new GetMemoryCommand(memoryId),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(entry, result);
    }

    /// <summary>
    /// Verifies that the handler returns null when the requested memory does not exist.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnNull_WhenMemoryDoesNotExist()
    {
        var memory = new TestMemory
        {
            Entry = new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Test memory",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        var handler = new GetMemoryCommandHandler(memory);

        var result = await handler.HandleAsync(
            new GetMemoryCommand(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that the handler passes the memory identifier to the memory service.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassMemoryId_ToMemory()
    {
        var memoryId = Guid.NewGuid();

        var memory = new TestMemory
        {
            Entry = new AtlasMemoryEntry
            {
                Id = memoryId,
                Content = "Test memory",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        var handler = new GetMemoryCommandHandler(memory);

        await handler.HandleAsync(
            new GetMemoryCommand(memoryId),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            memoryId,
            memory.ReceivedId);
    }

    /// <summary>
    /// Verifies that the handler passes the cancellation token to the memory service.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassCancellationToken_ToMemory()
    {
        var memory = new TestMemory();
        var handler = new GetMemoryCommandHandler(memory);

        using var cancellationTokenSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new GetMemoryCommand(Guid.NewGuid()),
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
            ThrowOnGet = true
        };

        var handler = new GetMemoryCommandHandler(memory);

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                new GetMemoryCommand(Guid.NewGuid()),
                cancellationTokenSource.Token));
    }

    private sealed class TestMemory : IAtlasMemory
    {
        public AtlasMemoryEntry? Entry { get; init; }

        public Guid ReceivedId { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public bool ThrowOnGet { get; init; }

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
            ReceivedId = id;
            ReceivedCancellationToken = cancellationToken;

            if (ThrowOnGet)
                throw new OperationCanceledException(cancellationToken);

            return Task.FromResult(
                Entry?.Id == id
                    ? Entry
                    : null);
        }

        public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);
        }

        public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
            AtlasMemoryQuery query,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);
        }
    }
}