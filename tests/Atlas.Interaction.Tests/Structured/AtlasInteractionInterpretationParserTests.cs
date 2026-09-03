using System.Text.Json;
using Atlas.Interaction.Models;
using Atlas.Interaction.Structured;
using Xunit;

namespace Atlas.Interaction.Tests.Structured;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionInterpretationParser"/> class.
/// </summary>
public sealed class AtlasInteractionInterpretationParserTests
{
    /// <summary>
    /// Verifies that structured JSON is parsed into an interaction interpretation.
    /// </summary>
    [Fact]
    public void Parse_Should_ReturnInteractionInterpretation()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var result = parser.Parse(content);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result.Interpretation.Intent);

        Assert.Equal(
            "camera",
            result.Interpretation.Query);

        Assert.Null(
            result.Interpretation.MemoryContent);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(
            result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that memory content is parsed correctly.
    /// </summary>
    [Fact]
    public void Parse_Should_ParseMemoryContent()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "StoreMemory",
                "query": null,
                "memoryContent": "I bought a Canon EOS 350D.",
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var result = parser.Parse(content);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result.Interpretation.Intent);

        Assert.Null(
            result.Interpretation.Query);

        Assert.Equal(
            "I bought a Canon EOS 350D.",
            result.Interpretation.MemoryContent);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(
            result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that JSON property names are treated case-insensitively.
    /// </summary>
    [Fact]
    public void Parse_Should_BeCaseInsensitive()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "Intent": "SearchMemory",
                "Query": "camera",
                "MemoryContent": null,
                "Confidence": "High",
                "IsAmbiguous": false
            }
            """;

        var result = parser.Parse(content);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result.Interpretation.Intent);

        Assert.Equal(
            "camera",
            result.Interpretation.Query);

        Assert.Equal(
            AtlasInteractionConfidence.High,
            result.Confidence);

        Assert.False(
            result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that invalid structured output is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenJsonIsInvalid()
    {
        var parser = new AtlasInteractionInterpretationParser();

        Assert.Throws<JsonException>(
            () => parser.Parse("not valid json"));
    }

    /// <summary>
    /// Verifies that empty content is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenContentIsEmpty()
    {
        var parser = new AtlasInteractionInterpretationParser();

        Assert.Throws<ArgumentException>(
            () => parser.Parse(string.Empty));
    }

    /// <summary>
    /// Verifies that invalid intent values are rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenIntentIsInvalid()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SomethingThatDoesNotExist",
                "query": "camera",
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        Assert.Throws<JsonException>(
            () => parser.Parse(content));
    }

    /// <summary>
    /// Verifies that a SearchMemory interpretation without a query is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenSearchMemoryHasNoQuery()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": null,
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "SearchMemory interpretations require a query.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a SearchMemory interpretation with memory content is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenSearchMemoryHasMemoryContent()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": "I bought a camera.",
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "SearchMemory interpretations must not contain memory content.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a StoreMemory interpretation without memory content is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenStoreMemoryHasNoMemoryContent()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "StoreMemory",
                "query": null,
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "StoreMemory interpretations require memory content.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a StoreMemory interpretation with a query is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenStoreMemoryHasQuery()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "StoreMemory",
                "query": "camera",
                "memoryContent": "I bought a camera.",
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "StoreMemory interpretations must not contain a query.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that an Unknown interpretation with a query is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenUnknownHasQuery()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "Unknown",
                "query": "camera",
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "Unknown interpretations must not contain a query.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that an Unknown interpretation with memory content is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenUnknownHasMemoryContent()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "Unknown",
                "query": null,
                "memoryContent": "I bought a camera.",
                "confidence": "High",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "Unknown interpretations must not contain memory content.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that confidence and ambiguity are parsed correctly.
    /// </summary>
    [Fact]
    public void Parse_Should_ParseConfidenceAndAmbiguity()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null,
                "confidence": "Medium",
                "isAmbiguous": true
            }
            """;

        var result = parser.Parse(content);

        Assert.Equal(
            AtlasInteractionConfidence.Medium,
            result.Confidence);

        Assert.True(
            result.IsAmbiguous);
    }

    /// <summary>
    /// Verifies that a high-confidence, ambiguous interpretation is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenHighConfidenceIsAmbiguous()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null,
                "confidence": "High",
                "isAmbiguous": true
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "An ambiguous interpretation cannot have high confidence.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a low-confidence, non-ambiguous interpretation is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenLowConfidenceIsNotAmbiguous()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null,
                "confidence": "Low",
                "isAmbiguous": false
            }
            """;

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(content));

        Assert.Equal(
            "A non-ambiguous interpretation cannot have low confidence.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that an invalid confidence value is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenConfidenceIsInvalid()
    {
        var parser = new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null,
                "confidence": "VeryHigh",
                "isAmbiguous": false
            }
            """;

        Assert.Throws<JsonException>(
            () => parser.Parse(content));
    }
}
