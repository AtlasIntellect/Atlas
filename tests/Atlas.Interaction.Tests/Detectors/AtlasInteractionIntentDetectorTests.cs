using Atlas.Interaction.Detectors;
using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Detectors;

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

        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(interaction);

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

        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(interaction);

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

        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(interaction);

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

        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(interaction);

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

        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(interaction);

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

        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(interaction);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionIntentDetector.Detect"/> method
    /// correctly identifies memory-related questions as <see cref="AtlasInteractionIntent.SearchMemory"/>.
    /// </summary>
    /// <param name="input">The input string representing a memory-related question.</param>
    [Theory]
    [InlineData("What camera do I have?")]
    [InlineData("What car did I buy?")]
    [InlineData("Which laptop do I own?")]
    [InlineData("Do you remember my favorite color?")]
    [InlineData("Can you tell me what phone I bought?")]
    [InlineData("Did I buy a camera?")]
    [InlineData("  What car did I buy?  ")]
    public void Detect_Should_ReturnSearchMemory_ForMemoryQuestions(
        string input)
    {
        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(
            new AtlasInteraction
            {
                Input = input
            });

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            result);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionIntentDetector.Detect"/> method 
    /// correctly identifies memory storage requests based on the provided input.
    /// </summary>
    /// <param name="input">The input string representing a memory storage request.</param>
    [Theory]
    [InlineData("Remember that I bought a camera.")]
    [InlineData("Store my favorite color.")]
    [InlineData("Save that I like pizza.")]
    public void Detect_Should_ReturnStoreMemory_ForMemoryStorageRequests(
        string input)
    {
        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(
            new AtlasInteraction
            {
                Input = input
            });

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            result);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionIntentDetector.Detect(AtlasInteraction)"/> method
    /// returns <see cref="AtlasInteractionIntent.Unknown"/> when the input consists of ordinary statements
    /// that do not indicate any specific intent.
    /// </summary>
    /// <param name="input">The input string representing an ordinary statement.</param>
    [Theory]
    [InlineData("I bought a camera yesterday.")]
    [InlineData("I like pizza.")]
    [InlineData("Hello Atlas.")]
    public void Detect_Should_ReturnUnknown_ForOrdinaryStatements(
        string input)
    {
        var detector = new AtlasInteractionIntentDetector();

        var result = detector.Detect(
            new AtlasInteraction
            {
                Input = input
            });

        Assert.Equal(
            AtlasInteractionIntent.Unknown,
            result);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasInteractionIntentDetector.Detect(AtlasInteraction)"/> method
    /// throws an <see cref="ArgumentNullException"/> when the provided interaction is <c>null</c>.
    /// </summary>
    [Fact]
    public void Detect_Should_Throw_WhenInteractionIsNull()
    {
        var detector = new AtlasInteractionIntentDetector();

        Assert.Throws<ArgumentNullException>(
            () => detector.Detect(null!));
    }
}