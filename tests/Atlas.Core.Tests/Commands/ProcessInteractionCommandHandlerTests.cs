using Atlas.Abstractions.Interaction;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

/// <summary>
/// Provides unit tests for the <see cref="ProcessInteractionCommandHandler"/> class.
/// </summary>
public sealed class ProcessInteractionCommandHandlerTests
{
    /// <summary>
    /// Verifies that the handler returns the response produced by the processor.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnProcessorResponse()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var response = new AtlasResponse
        {
            Content = "Hello from Atlas"
        };

        var processor = new TestProcessor
        {
            Response = response
        };

        var handler = new ProcessInteractionCommandHandler(processor);

        var result = await handler.HandleAsync(
            new ProcessInteractionCommand(interaction),
            TestContext.Current.CancellationToken);

        Assert.Same(
            response,
            result);
    }

    /// <summary>
    /// Verifies that the handler passes the interaction to the processor.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassInteraction_ToProcessor()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var processor = new TestProcessor();

        var handler = new ProcessInteractionCommandHandler(processor);

        await handler.HandleAsync(
            new ProcessInteractionCommand(interaction),
            TestContext.Current.CancellationToken);

        Assert.Same(
            interaction,
            processor.ReceivedInteraction);
    }

    /// <summary>
    /// Verifies that the handler passes the cancellation token to the processor.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassCancellationToken_ToProcessor()
    {
        var processor = new TestProcessor();
        var handler = new ProcessInteractionCommandHandler(processor);

        using var cancellationTokenSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new ProcessInteractionCommand(
                new AtlasInteraction
                {
                    Input = "Hello Atlas"
                }),
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            processor.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that the handler propagates cancellation from the processor.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PropagateCancellation()
    {
        var processor = new TestProcessor
        {
            ThrowOnProcess = true
        };

        var handler = new ProcessInteractionCommandHandler(processor);

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                new ProcessInteractionCommand(
                    new AtlasInteraction
                    {
                        Input = "Hello Atlas"
                    }),
                cancellationTokenSource.Token));
    }

    private sealed class TestProcessor : IAtlasInteractionProcessor
    {
        public AtlasResponse? Response { get; init; }

        public AtlasInteraction? ReceivedInteraction { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public bool ThrowOnProcess { get; init; }

        public Task<AtlasResponse> ProcessAsync(
            AtlasInteraction interaction,
            CancellationToken cancellationToken = default)
        {
            ReceivedInteraction = interaction;
            ReceivedCancellationToken = cancellationToken;

            if (ThrowOnProcess)
                throw new OperationCanceledException(cancellationToken);

            return Task.FromResult(
                Response ?? new AtlasResponse
                {
                    Content = "Test response"
                });
        }
    }
}