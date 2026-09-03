using Atlas.Interaction.Detectors;
using Atlas.Interaction.Extractors;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Interpreters;
using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Interpreters;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionInterpreter"/> class.
/// </summary>
public sealed class AtlasInteractionInterpreterTests
{
    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionInterpreter.InterpretAsync"/> method correctly interprets an
    /// <see cref="AtlasInteraction"/> instance with an input query, returning an interpretation with
    /// <see cref="AtlasInteractionIntent.SearchMemory"/> intent and the extracted query.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnSearchMemoryWithQuery()
    {
        var intentDetector = new AtlasInteractionIntentDetector();

        var queryExtractor = new AtlasInteractionQueryExtractor();

        var memoryContentExtractor = new AtlasInteractionMemoryContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                memoryContentExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var result =
            await interpreter.InterpretAsync(interaction, TestContext.Current.CancellationToken);

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
    /// Verifies that the <see cref="AtlasInteractionInterpreter"/> correctly interprets an interaction
    /// with an input that implies storing memory, returning the intent as <see cref="AtlasInteractionIntent.StoreMemory"/>
    /// and ensuring that no query is extracted.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnStoreMemoryWithoutQuery()
    {
        var intentDetector = new AtlasInteractionIntentDetector();

        var queryExtractor = new AtlasInteractionQueryExtractor();

        var memoryContentExtractor = new AtlasInteractionMemoryContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                memoryContentExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon camera."
        };

        var result =
            await interpreter.InterpretAsync(interaction, TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result.Interpretation.Intent);

        Assert.Null(result.Interpretation.Query);

        Assert.Equal(
            "I bought a Canon camera.",
            result.Interpretation.MemoryContent);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionInterpreter.InterpretAsync"/> method
    /// throws an <see cref="ArgumentNullException"/> when the provided interaction is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// This test ensures that the <see cref="AtlasInteractionInterpreter"/> correctly handles
    /// invalid input by throwing the appropriate exception.
    /// </remarks>
    [Fact]
    public async Task InterpretAsync_Should_Throw_WhenInteractionIsNull()
    {
        var intentDetector = new AtlasInteractionIntentDetector();

        var queryExtractor = new AtlasInteractionQueryExtractor();

        var memoryContentExtractor = new AtlasInteractionMemoryContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                memoryContentExtractor);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => interpreter.InterpretAsync(null!, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Tests that the <see cref="AtlasInteractionInterpreter.InterpretAsync(AtlasInteraction, CancellationToken)"/> method
    /// correctly interprets an interaction with the intent to store memory and returns the expected memory content.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnMemoryContentForStoreMemory()
    {
        var intentDetector = new AtlasInteractionIntentDetector();

        var queryExtractor = new AtlasInteractionQueryExtractor();

        var memoryContentExtractor = new AtlasInteractionMemoryContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                memoryContentExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D camera."
        };

        var result =
            await interpreter.InterpretAsync(interaction, TestContext.Current.CancellationToken);

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
    /// Tests that the <see cref="AtlasInteractionInterpreter.InterpretAsync"/> method
    /// returns an interpretation with an <see cref="AtlasInteractionIntent.Unknown"/> intent,
    /// and no query or memory content, when the input interaction contains an unknown intent.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnNoContentForUnknownIntent()
    {
        var intentDetector = new AtlasInteractionIntentDetector();

        var queryExtractor = new AtlasInteractionQueryExtractor();

        var memoryContentExtractor = new AtlasInteractionMemoryContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                memoryContentExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas."
        };

        var result =
            await interpreter.InterpretAsync(interaction, TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionIntent.Unknown,
            result.Interpretation.Intent);

        Assert.Equal(
            AtlasInteractionConfidence.Low,
            result.Confidence);

        Assert.True(result.IsAmbiguous);

        Assert.Null(result.Interpretation.Query);

        Assert.Null(result.Interpretation.MemoryContent);
    }

    /// <summary>
    /// Verifies that search interactions use the query extractor and do not extract
    /// memory content.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ExtractQuery_ForSearchMemory()
    {
        var intentDetector = new TestIntentDetector
        {
            Intent = AtlasInteractionIntent.SearchMemory
        };

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var contentExtractor = new TestContentExtractor
        {
            Content = "Should not be extracted."
        };

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                contentExtractor);

        var result = await interpreter.InterpretAsync(
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            }, TestContext.Current.CancellationToken);

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

        Assert.True(queryExtractor.WasCalled);

        Assert.False(contentExtractor.WasCalled);
    }

    /// <summary>
    /// Verifies that store interactions use the memory-content extractor and do not
    /// extract a search query.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ExtractMemoryContent_ForStoreMemory()
    {
        var intentDetector = new TestIntentDetector
        {
            Intent = AtlasInteractionIntent.StoreMemory
        };

        var queryExtractor = new TestQueryExtractor
        {
            Query = "Should not be extracted."
        };

        var contentExtractor = new TestContentExtractor
        {
            Content = "I bought a Canon EOS 350D."
        };

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                contentExtractor);

        var result = await interpreter.InterpretAsync(
            new AtlasInteraction
            {
                Input = "Remember that I bought a Canon EOS 350D."
            }, TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result.Interpretation.Intent);

        Assert.Null(result.Interpretation.Query);

        Assert.Equal(
            "I bought a Canon EOS 350D.",
            result.Interpretation.MemoryContent);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(result.IsAmbiguous);

        Assert.False(queryExtractor.WasCalled);

        Assert.True(contentExtractor.WasCalled);
    }

    /// <summary>
    /// Verifies that unknown interactions do not invoke either query or memory-content extraction.
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_NotExtractData_ForUnknownIntent()
    {
        var intentDetector = new TestIntentDetector
        {
            Intent = AtlasInteractionIntent.Unknown
        };

        var queryExtractor = new TestQueryExtractor();
        var contentExtractor = new TestContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                contentExtractor);

        var result = await interpreter.InterpretAsync(
            new AtlasInteraction
            {
                Input = "Hello Atlas."
            }, TestContext.Current.CancellationToken);

        Assert.Equal(
            AtlasInteractionIntent.Unknown,
            result.Interpretation.Intent);

        Assert.Equal(
            AtlasInteractionConfidence.Low,
            result.Confidence);

        Assert.True(result.IsAmbiguous);

        Assert.False(queryExtractor.WasCalled);

        Assert.False(contentExtractor.WasCalled);

        Assert.Null(result.Interpretation.Query);

        Assert.Null(result.Interpretation.MemoryContent);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionInterpreter.InterpretAsync"/> method returns a valid result
    /// </summary>
    [Fact]
    public async Task InterpretAsync_Should_ReturnValidResult()
    {
        var intentDetector = new AtlasInteractionIntentDetector();

        var queryExtractor = new AtlasInteractionQueryExtractor();

        var memoryContentExtractor = new AtlasInteractionMemoryContentExtractor();

        var interpreter =
            new AtlasInteractionInterpreter(
                intentDetector,
                queryExtractor,
                memoryContentExtractor);

        var result =
            await interpreter.InterpretAsync(
                new AtlasInteraction
                {
                    Input = "Hello Atlas."
                },
                TestContext.Current.CancellationToken);

        result.Validate();
    }

    private sealed class TestIntentDetector
        : IAtlasInteractionIntentDetector
    {
        public AtlasInteractionIntent Intent { get; init; }

        public AtlasInteractionIntent Detect(AtlasInteraction interaction)
        {
            return Intent;
        }
    }

    private sealed class TestQueryExtractor
        : IAtlasInteractionQueryExtractor
    {
        public string Query { get; init; } = "camera";

        public bool WasCalled { get; private set; }

        public string ExtractQuery(AtlasInteraction interaction)
        {
            WasCalled = true;

            return Query;
        }
    }

    private sealed class TestContentExtractor
        : IAtlasInteractionMemoryContentExtractor
    {
        public string Content { get; init; } = "Test memory.";

        public bool WasCalled { get; private set; }

        public string ExtractContent(AtlasInteraction interaction)
        {
            WasCalled = true;

            return Content;
        }
    }
}
