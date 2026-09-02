using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;
using Atlas.Interaction.Processors;
using Xunit;

namespace Atlas.Interaction.Tests.Processors;

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

        var handlers = new[]
        {
            handler
        };

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation =
                    new AtlasInteractionInterpretation(
                        AtlasInteractionIntent.SearchMemory,
                        "camera",
                        null)
            };

        var processor = new AtlasInteractionProcessor(
            interactionInterpreter,
            handlers);

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

        var handlers = new[]
        {
            handler
        };

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation =
                    new AtlasInteractionInterpretation(
                        AtlasInteractionIntent.SearchMemory,
                        "camera",
                        null)
            };

        var processor = new AtlasInteractionProcessor(
            interactionInterpreter,
            handlers);

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

        var handlers = new[]
        {
            unrelatedHandler,
            searchHandler
        };

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation =
                    new AtlasInteractionInterpretation(
                        AtlasInteractionIntent.SearchMemory,
                        "camera",
                        null)
            };

        var processor = new AtlasInteractionProcessor(
            interactionInterpreter,
            handlers);

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
        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation =
                    new AtlasInteractionInterpretation(
                        AtlasInteractionIntent.Unknown,
                        null,
                        null)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                []);

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

        var handlers = new[]
        {
            handler
        };

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation =
                    new AtlasInteractionInterpretation(
                        AtlasInteractionIntent.SearchMemory,
                        "camera",
                        null)
            };

        var processor = new AtlasInteractionProcessor(
            interactionInterpreter,
            handlers);

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

        var handlers = new[]
        {
            handler
        };

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation =
                    new AtlasInteractionInterpretation(
                        AtlasInteractionIntent.SearchMemory,
                        "camera",
                        null)
            };

        var processor = new AtlasInteractionProcessor(
            interactionInterpreter,
            handlers);

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

    /// <summary>
    /// Verifies that the processor passes the interpretation to the selected handler.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_PassInterpretation_ToHandler()
    {
        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                Interpretation = interpretation
            };

        var handler =
            new TestInteractionHandler(
                AtlasInteractionIntent.SearchMemory);

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                [handler]);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Same(
            interpretation,
            handler.ReceivedInterpretation);
    }

    private sealed class TestInteractionHandler(
        AtlasInteractionIntent intent)
        : IAtlasInteractionHandler
    {
        public AtlasInteractionInterpretation? ReceivedInterpretation { get; private set; }

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
            AtlasInteractionInterpretation interpretation,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedInterpretation = interpretation;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(Response);
        }
    }

    private sealed class TestInteractionInterpreter
        : IAtlasInteractionInterpreter
    {
        public AtlasInteractionInterpretation Interpretation { get; init; } =
            new(
                AtlasInteractionIntent.Unknown,
                null,
                null);

        public AtlasInteractionInterpretation Interpret(
            AtlasInteraction interaction)
        {
            return Interpretation;
        }
    }
}