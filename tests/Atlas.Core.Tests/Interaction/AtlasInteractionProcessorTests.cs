using Atlas.Abstractions.Interaction;
using Atlas.Core.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionProcessor"/> class.
/// </summary>
public sealed class AtlasInteractionProcessorTests
{
    /// <summary>
    /// Verifies that the processor invokes the handler matching the detected intent.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_InvokeMatchingHandler()
    {
        var handler = new TestInteractionHandler(
            AtlasInteractionIntent.SearchMemory);

        var processor =
            new AtlasInteractionProcessor([handler]);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.True(handler.WasCalled);
    }

    /// <summary>
    /// Verifies that the processor returns the response produced by the handler.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_ReturnHandlerResponse()
    {
        var handler = new TestInteractionHandler(
            AtlasInteractionIntent.SearchMemory)
        {
            Response = new AtlasResponse
            {
                Content = "Test handler response."
            }
        };

        var processor =
            new AtlasInteractionProcessor([handler]);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "Test handler response.",
            response.Content);
    }

    /// <summary>
    /// Verifies that the processor selects the handler matching the detected intent
    /// instead of invoking unrelated handlers.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_SelectHandlerMatchingIntent()
    {
        var unrelatedHandler = new TestInteractionHandler(
            AtlasInteractionIntent.StoreMemory);

        var searchHandler = new TestInteractionHandler(
            AtlasInteractionIntent.SearchMemory);

        var processor =
            new AtlasInteractionProcessor(
            [
                unrelatedHandler,
                searchHandler
            ]);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.False(unrelatedHandler.WasCalled);
        Assert.True(searchHandler.WasCalled);
    }

    /// <summary>
    /// Ensures that the <see cref="AtlasInteractionProcessor.ProcessAsync"/> method
    /// throws an <see cref="InvalidOperationException"/> when no handler is registered
    /// for the detected intent.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_Throw_WhenNoHandlerExists()
    {
        var processor =
            new AtlasInteractionProcessor(
                Array.Empty<IAtlasInteractionHandler>());

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => processor.ProcessAsync(
                    interaction,
                    TestContext.Current.CancellationToken));

        Assert.Contains(
            "No interaction handler registered for intent",
            exception.Message);
    }

    /// <summary>
    /// Verifies that the processor passes the cancellation token to the selected handler.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_PassCancellationToken_ToHandler()
    {
        var handler = new TestInteractionHandler(
            AtlasInteractionIntent.SearchMemory);

        var processor =
            new AtlasInteractionProcessor([handler]);

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await processor.ProcessAsync(
            interaction,
            cancellationToken);

        Assert.Equal(
            cancellationToken,
            handler.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that the processor throws when cancellation has already been requested.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_Throw_WhenCancellationRequested()
    {
        var handler = new TestInteractionHandler(
            AtlasInteractionIntent.SearchMemory);

        var processor =
            new AtlasInteractionProcessor([handler]);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => processor.ProcessAsync(
                interaction,
                cancellationTokenSource.Token));
    }

    private sealed class TestInteractionHandler(
        AtlasInteractionIntent intent)
        : IAtlasInteractionHandler
    {
        public AtlasInteractionIntent Intent =>
            intent;

        public bool WasCalled { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public AtlasResponse Response { get; init; } = new()
        {
            Content = "Test response."
        };

        public Task<AtlasResponse> HandleAsync(
            AtlasInteraction interaction,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(Response);
        }
    }
}