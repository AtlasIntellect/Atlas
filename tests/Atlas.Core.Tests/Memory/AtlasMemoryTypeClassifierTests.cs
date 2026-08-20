using Atlas.Abstractions.Memory;
using Atlas.Core.Memory;
using Xunit;

namespace Atlas.Core.Tests.Memory;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemoryTypeClassifier"/> class.
/// </summary>
public sealed class AtlasMemoryTypeClassifierTests
{
    /// <summary>
    /// Verifies that factual memory content is classified as a fact.
    /// </summary>
    [Fact]
    public void Classify_Should_ReturnFact_ForFactualMemory()
    {
        var classifier = new AtlasMemoryTypeClassifier();

        var result = classifier.Classify(
            "I bought a Canon EOS 350D camera.");

        Assert.Equal(
            AtlasMemoryType.Fact,
            result);
    }

    /// <summary>
    /// Verifies that unknown content falls back to a fact.
    /// </summary>
    [Fact]
    public void Classify_Should_DefaultToFact_ForUnknownContent()
    {
        var classifier = new AtlasMemoryTypeClassifier();

        var result = classifier.Classify(
            "Something Atlas does not recognize.");

        Assert.Equal(
            AtlasMemoryType.Fact,
            result);
    }

    /// <summary>
    /// Verifies that whitespace surrounding the content does not affect
    /// classification.
    /// </summary>
    [Fact]
    public void Classify_Should_IgnoreSurroundingWhitespace()
    {
        var classifier = new AtlasMemoryTypeClassifier();

        var result = classifier.Classify(
            "   I bought a Canon EOS 350D camera.   ");

        Assert.Equal(
            AtlasMemoryType.Fact,
            result);
    }
}