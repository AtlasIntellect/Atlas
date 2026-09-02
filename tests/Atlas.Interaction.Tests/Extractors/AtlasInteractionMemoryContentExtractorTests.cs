using Atlas.Interaction.Extractors;
using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Extractors;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionMemoryContentExtractor"/> class.
/// </summary>
public sealed class AtlasInteractionMemoryContentExtractorTests
{
    /// <summary>
    /// Verifies that the extractor removes the "remember that" prefix.
    /// </summary>
    [Fact]
    public void ExtractContent_Should_RemoveRememberThatPrefix()
    {
        var extractor = new AtlasInteractionMemoryContentExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D camera."
        };

        var result = extractor.ExtractContent(interaction);

        Assert.Equal(
            "I bought a Canon EOS 350D camera.",
            result);
    }

    /// <summary>
    /// Verifies that the extractor removes the "remember" prefix.
    /// </summary>
    [Fact]
    public void ExtractContent_Should_RemoveRememberPrefix()
    {
        var extractor = new AtlasInteractionMemoryContentExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "Remember I have a dog."
        };

        var result = extractor.ExtractContent(interaction);

        Assert.Equal(
            "I have a dog.",
            result);
    }

    /// <summary>
    /// Verifies that the extractor removes the "store that" prefix.
    /// </summary>
    [Fact]
    public void ExtractContent_Should_RemoveStoreThatPrefix()
    {
        var extractor = new AtlasInteractionMemoryContentExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "Store that my favorite color is blue."
        };

        var result = extractor.ExtractContent(interaction);

        Assert.Equal(
            "my favorite color is blue.",
            result);
    }

    /// <summary>
    /// Verifies that the extractor removes the "save that" prefix.
    /// </summary>
    [Fact]
    public void ExtractContent_Should_RemoveSaveThatPrefix()
    {
        var extractor = new AtlasInteractionMemoryContentExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "Save that I live in Ouddorp."
        };

        var result = extractor.ExtractContent(interaction);

        Assert.Equal(
            "I live in Ouddorp.",
            result);
    }

    /// <summary>
    /// Verifies that normal statements are preserved.
    /// </summary>
    [Fact]
    public void ExtractContent_Should_PreserveNormalStatement()
    {
        var extractor = new AtlasInteractionMemoryContentExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "I bought a Canon EOS 350D camera."
        };

        var result = extractor.ExtractContent(interaction);

        Assert.Equal(
            "I bought a Canon EOS 350D camera.",
            result);
    }

    /// <summary>
    /// Verifies that surrounding whitespace is removed.
    /// </summary>
    [Fact]
    public void ExtractContent_Should_TrimWhitespace()
    {
        var extractor = new AtlasInteractionMemoryContentExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "  Remember that I bought a camera.  "
        };

        var result = extractor.ExtractContent(interaction);

        Assert.Equal(
            "I bought a camera.",
            result);
    }
}