using Atlas.Interaction.Models;
using Atlas.Interaction.Structured;
using System.Text.Json;
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
        var parser =
            new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SearchMemory",
                "query": "camera",
                "memoryContent": null
            }
            """;

        var result =
            parser.Parse(content);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result.Intent);

        Assert.Equal(
            "camera",
            result.Query);

        Assert.Null(
            result.MemoryContent);
    }

    /// <summary>
    /// Verifies that memory content is parsed correctly.
    /// </summary>
    [Fact]
    public void Parse_Should_ParseMemoryContent()
    {
        var parser =
            new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "StoreMemory",
                "query": null,
                "memoryContent": "I bought a Canon EOS 350D."
            }
            """;

        var result =
            parser.Parse(content);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result.Intent);

        Assert.Null(
            result.Query);

        Assert.Equal(
            "I bought a Canon EOS 350D.",
            result.MemoryContent);
    }

    /// <summary>
    /// Verifies that JSON property names are treated case-insensitively.
    /// </summary>
    [Fact]
    public void Parse_Should_BeCaseInsensitive()
    {
        var parser =
            new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "Intent": "SearchMemory",
                "Query": "camera",
                "MemoryContent": null
            }
            """;

        var result =
            parser.Parse(content);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result.Intent);

        Assert.Equal(
            "camera",
            result.Query);
    }

    /// <summary>
    /// Verifies that invalid structured output is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenJsonIsInvalid()
    {
        var parser =
            new AtlasInteractionInterpretationParser();

        Assert.Throws<JsonException>(
            () => parser.Parse("not valid json"));
    }

    /// <summary>
    /// Verifies that empty content is rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenContentIsEmpty()
    {
        var parser =
            new AtlasInteractionInterpretationParser();

        Assert.Throws<ArgumentException>(
            () => parser.Parse(string.Empty));
    }

    /// <summary>
    /// Verifies that invalid intent values are rejected.
    /// </summary>
    [Fact]
    public void Parse_Should_Throw_WhenIntentIsInvalid()
    {
        var parser =
            new AtlasInteractionInterpretationParser();

        const string content =
            """
            {
                "intent": "SomethingThatDoesNotExist",
                "query": "camera",
                "memoryContent": null
            }
           """;

        Assert.Throws<JsonException>(
            () => parser.Parse(content));
    }
}
