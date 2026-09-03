using Atlas.AI.Interfaces;
using Atlas.AI.Structured;
using Atlas.Interaction.Interpreters;
using Atlas.Interaction.Models;
using Atlas.Interaction.Structured;
using System.Text.Json;
using Xunit;

namespace Atlas.Interaction.Tests.Interpreters;

/// <summary>
/// Provides unit tests for the <see cref="AtlasLanguageModelInteractionInterpreter"/> class.
/// </summary>
public sealed class AtlasLanguageModelInteractionInterpreterTests
{
    /// <summary>
    /// Verifies that the interpreter returns the interpretation parsed from the language-model response.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnParsedInterpretationResult()
    {
        var languageModel =
        new TestStructuredLanguageModel
        {
            Response = new AtlasStructuredLanguageModelResponse(
        """
            {
            "intent": "SearchMemory",
            "query": "camera",
            "memoryContent": null,
            "confidence": "High",
            "isAmbiguous": false
            }
            """)
        };

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        var result =
            await interpreter.InterpretAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result.Interpretation.Intent);

        Assert.Equal(
            "camera",
            result.Interpretation.Query);

        Assert.Null(result.Interpretation.MemoryContent);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that the interpreter sends the interaction to the language model
    /// as a structured request with the expected response type.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_SendStructuredRequest()
    {
        var languageModel = new TestStructuredLanguageModel();

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        var interaction =
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            };

        await interpreter.InterpretAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var request =
            languageModel.ReceivedRequest;

        Assert.NotNull(request);

        Assert.Equal(
            typeof(AtlasStructuredInteractionInterpretation),
            request.ResponseType);

        Assert.Contains(
            interaction.Input,
            request.Prompt);

        Assert.Contains(
            "intent",
            request.Prompt,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "query",
            request.Prompt,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "memoryContent",
            request.Prompt,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "confidence",
            request.Prompt,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "isAmbiguous",
            request.Prompt,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that confidence and ambiguity returned by the language model
    /// are preserved in the interpretation result.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_PreserveConfidenceAndAmbiguity()
    {
        var languageModel =
            new TestStructuredLanguageModel
            {
                Response =
                    new AtlasStructuredLanguageModelResponse(
                        """
                    {
                        "intent": "SearchMemory",
                        "query": "camera",
                        "memoryContent": null,
                        "confidence": "Medium",
                        "isAmbiguous": true
                    }
                    """)
            };

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        var result =
            await interpreter.InterpretAsync(
                new AtlasInteraction
                {
                    Input = "What camera?"
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionConfidence.Medium,
            result.Confidence);

        Assert.True(result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that a StoreMemory interpretation is returned correctly.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnStoreMemoryInterpretation()
    {
        var languageModel =
            new TestStructuredLanguageModel
            {
                Response =
                    new AtlasStructuredLanguageModelResponse(
                        """
                    {
                        "intent": "StoreMemory",
                        "query": null,
                        "memoryContent": "I bought a Canon EOS 350D camera.",
                        "confidence": "High",
                        "isAmbiguous": false
                    }
                    """)
            };

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        var result =
            await interpreter.InterpretAsync(
                new AtlasInteraction
                {
                    Input = "I bought a Canon EOS 350D camera."
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result.Interpretation.Intent);

        Assert.Null(result.Interpretation.Query);

        Assert.Equal(
            "I bought a Canon EOS 350D camera.",
            result.Interpretation.MemoryContent);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that the cancellation token is passed to the language model.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_PassCancellationToken()
    {
        var languageModel = new TestStructuredLanguageModel();

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        await interpreter.InterpretAsync(
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            },
            cancellationToken);

        Assert.Equal(
            cancellationToken,
            languageModel.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that an already-cancelled operation is rejected.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_Throw_WhenCancellationRequested()
    {
        var languageModel = new TestStructuredLanguageModel();

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => interpreter.InterpretAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that a null interaction is rejected.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_Throw_WhenInteractionIsNull()
    {
        var languageModel = new TestStructuredLanguageModel();

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => interpreter.InterpretAsync(
                null!,
                TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Verifies that malformed language-model output is propagated as a parsing error.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_Throw_WhenLanguageModelOutputIsInvalid()
    {
        var languageModel =
            new TestStructuredLanguageModel
            {
                Response =
                    new AtlasStructuredLanguageModelResponse(
                        "not valid json")
            };

        var parser = new AtlasInteractionInterpretationParser();

        var interpreter =
            new AtlasLanguageModelInteractionInterpreter(
                languageModel,
                parser);

        await Assert.ThrowsAsync<JsonException>(
            () => interpreter.InterpretAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                TestContext.Current.CancellationToken));
    }

    private sealed class TestStructuredLanguageModel
        : IAtlasStructuredLanguageModel
    {
        public AtlasStructuredLanguageModelRequest? ReceivedRequest { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public AtlasStructuredLanguageModelResponse Response { get; init; } = new(
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": false
            }
            """);

        public Task<AtlasStructuredLanguageModelResponse> GenerateAsync(
            AtlasStructuredLanguageModelRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            cancellationToken.ThrowIfCancellationRequested();

            ReceivedRequest = request;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(Response);
        }
    }
}
