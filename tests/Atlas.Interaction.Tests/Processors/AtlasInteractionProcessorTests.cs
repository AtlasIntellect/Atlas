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
        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var handlers = new[]
        {
            handler
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                handlers);

        var interaction =
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.True(
            handler.WasCalled);
    }

    /// <summary>
    /// Verifies that the processor returns the response produced by the handler.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_ReturnHandlerResponse()
    {
        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var handlers = new[]
        {
            handler
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                handlers);

        var interaction =
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            };

        var response =
            await processor.ProcessAsync(
                interaction,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            "Handler response",
            response.Content);
    }

    /// <summary>
    /// Verifies that the processor selects the handler matching the detected intent
    /// instead of invoking unrelated handlers.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_SelectHandlerMatchingIntent()
    {
        var unrelatedHandler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.StoreMemory
            };

        var searchHandler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var handlers = new[]
        {
            unrelatedHandler,
            searchHandler
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                handlers);

        var interaction =
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.False(
            unrelatedHandler.WasCalled);

        Assert.True(
            searchHandler.WasCalled);
    }

    /// <summary>
    /// Ensures that the <see cref="AtlasInteractionProcessor.ProcessAsync"/> method
    /// throws an <see cref="InvalidOperationException"/> when no handler is registered
    /// for the interpreted intent.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_Throw_WhenNoHandlerExists()
    {
        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                []);

        var interaction =
            new AtlasInteraction
            {
                Input = "What camera do I have?"
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
        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var handlers = new[]
        {
            handler
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                handlers);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var interaction =
            new AtlasInteraction
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
        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var handlers = new[]
        {
            handler
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interactionInterpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                handlers);

        var interaction =
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            };

        using var cancellationTokenSource =
            new CancellationTokenSource();

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
                InterpretationResult =
                    new AtlasInteractionInterpretationResult(
                        interpretation,
                        AtlasInteractionConfidence.High,
                        false)
            };

        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var processor =
            new AtlasInteractionProcessor(
                interactionInterpreter,
                [handler]);

        var interaction =
            new AtlasInteraction
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

    /// <summary>
    /// Verifies that the processor returns a clarification response when the interpretation is ambiguous.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_ReturnClarificationResponse_WhenInterpretationIsAmbiguous()
    {
        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interpretationResult =
            new AtlasInteractionInterpretationResult(
                interpretation,
                AtlasInteractionConfidence.Medium,
                true);

        var interpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult = interpretationResult
            };

        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var processor =
            new AtlasInteractionProcessor(
                interpreter,
                [handler]);

        var response =
            await processor.ProcessAsync(
                new AtlasInteraction
                {
                    Input = "What camera?"
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(
            "I'm not quite sure what you mean. Could you clarify?",
            response.Content);

        Assert.False(
            handler.WasCalled);
    }

    /// <summary>
    /// Verifies that the processor invokes the handler when the interpretation is not ambiguous.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_InvokeHandler_WhenInterpretationIsNotAmbiguous()
    {
        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null);

        var interpretationResult =
            new AtlasInteractionInterpretationResult(
                interpretation,
                AtlasInteractionConfidence.High,
                false);

        var interpreter =
            new TestInteractionInterpreter
            {
                InterpretationResult = interpretationResult
            };

        var handler =
            new TestInteractionHandler
            {
                Intent = AtlasInteractionIntent.SearchMemory
            };

        var processor =
            new AtlasInteractionProcessor(
                interpreter,
                [handler]);

        var response =
            await processor.ProcessAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                TestContext.Current.CancellationToken);

        Assert.True(
            handler.WasCalled);

        Assert.Equal(
            "Handler response",
            response.Content);
    }

    private sealed class TestInteractionInterpreter
        : IAtlasInteractionInterpreter
    {
        public AtlasInteractionInterpretationResult InterpretationResult { get; init; } =
            new(
                new AtlasInteractionInterpretation(
                    AtlasInteractionIntent.Unknown,
                    null,
                    null),
                AtlasInteractionConfidence.Low,
                true);

        public Task<AtlasInteractionInterpretationResult> InterpretAsync(
            AtlasInteraction interaction,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                InterpretationResult);
        }
    }

    private sealed class TestInteractionHandler
        : IAtlasInteractionHandler
    {
        public AtlasInteractionIntent Intent { get; init; }

        public bool WasCalled { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public AtlasInteractionInterpretation? ReceivedInterpretation { get; private set; }

        public Task<AtlasResponse> HandleAsync(
            AtlasInteraction interaction,
            AtlasInteractionInterpretation interpretation,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;
            ReceivedInterpretation = interpretation;

            return Task.FromResult(
                new AtlasResponse
                {
                    Content = "Handler response"
                });
        }
    }
}