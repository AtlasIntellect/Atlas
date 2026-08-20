using Atlas.Abstractions.Interaction;
using Atlas.Core.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionIntentDetector"/> class.
/// </summary>
public sealed class AtlasInteractionIntentDetectorTests
{
    /// <summary>
    /// Verifies that an interaction mentioning a camera is detected as a memory search.
    /// </summary>
    [Fact]
    public void Detect_Should_ReturnSearchMemory_WhenInputMentionsCamera()
    {
        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result);
    }

    /// <summary>
    /// Verifies that intent detection is case insensitive.
    /// </summary>
    [Fact]
    public void Detect_Should_BeCaseInsensitive()
    {
        var interaction = new AtlasInteraction
        {
            Input = "WHAT CAMERA DO I HAVE?"
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result);
    }

    /// <summary>
    /// Verifies that unrelated interactions return an unknown intent.
    /// </summary>
    [Fact]
    public void Detect_Should_ReturnUnknown_WhenInputDoesNotMentionCamera()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.Unknown,
            result);
    }

    /// <summary>
    /// Verifies that surrounding whitespace does not affect intent detection.
    /// </summary>
    [Fact]
    public void Detect_Should_IgnoreSurroundingWhitespace()
    {
        var interaction = new AtlasInteraction
        {
            Input = "   What camera do I have?   "
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result);
    }

    /// <summary>
    /// Verifies that an interaction beginning with "remember" is detected as a memory store.
    /// </summary>
    [Fact]
    public void Detect_Should_ReturnStoreMemory_WhenInputStartsWithRemember()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D."
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result);
    }

    /// <summary>
    /// Verifies that an interaction beginning with "store" is detected as a memory store.
    /// </summary>
    [Fact]
    public void Detect_Should_ReturnStoreMemory_WhenInputStartsWithStore()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Store this memory."
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result);
    }

    /// <summary>
    /// Verifies that store-memory detection is case insensitive.
    /// </summary>
    [Fact]
    public void Detect_Should_BeCaseInsensitiveForStoreMemory()
    {
        var interaction = new AtlasInteraction
        {
            Input = "REMEMBER THAT I BOUGHT A CANON EOS 350D."
        };

        var result =
            AtlasInteractionIntentDetector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result);
    }
}