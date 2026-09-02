using Atlas.Memory.Classifiers;
using Atlas.Memory.Models;
using Xunit;

namespace Atlas.Memory.Tests.Classifiers;

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

    /// <summary>
    /// Verifies that preference statements are classified as preferences.
    /// </summary>
    [Theory]
    [InlineData("My favorite color is blue.")]
    [InlineData("My favorite food is pizza.")]
    [InlineData("I prefer dark green.")]
    [InlineData("I like pizza.")]
    [InlineData("I don't like mushrooms.")]
    [InlineData("I dislike noisy restaurants.")]
    public void Classify_Should_ReturnPreference_ForPreferenceStatements(
        string content)
    {
        var classifier = new AtlasMemoryTypeClassifier();

        var result = classifier.Classify(content);

        Assert.Equal(
            AtlasMemoryType.Preference,
            result);
    }

    /// <summary>
    /// Verifies that ordinary factual statements and non-task statements
    /// are classified as facts.
    /// </summary>
    [Theory]
    [InlineData("I bought a Canon EOS 350D camera.")]
    [InlineData("I have a Canon camera.")]
    [InlineData("I visited Amsterdam yesterday.")]
    [InlineData("I need a new camera.")]
    [InlineData("I have a meeting tomorrow.")]
    [InlineData("I bought groceries yesterday.")]
    public void Classify_Should_ReturnFact_ForFactualStatements(
        string content)
    {
        var classifier = new AtlasMemoryTypeClassifier();

        var result = classifier.Classify(content);

        Assert.Equal(
            AtlasMemoryType.Fact,
            result);
    }

    /// <summary>
    /// Verifies that task statements are classified as tasks.
    /// </summary>
    [Theory]
    [InlineData("Remind me to buy milk.")]
    [InlineData("I need to buy groceries.")]
    [InlineData("I have to call the dentist.")]
    [InlineData("I must finish the Atlas tests.")]
    [InlineData("Don't forget to pick up the package.")]
    public void Classify_Should_ReturnTask_ForTaskStatements(
        string content)
    {
        var classifier = new AtlasMemoryTypeClassifier();

        var result = classifier.Classify(content);

        Assert.Equal(
            AtlasMemoryType.Task,
            result);
    }

    /// <summary>
    /// Verifies that null content is rejected.
    /// </summary>
    [Fact]
    public void Classify_Should_ThrowArgumentNullException_ForNullContent()
    {
        var classifier = new AtlasMemoryTypeClassifier();

        Assert.Throws<ArgumentNullException>(
            () => classifier.Classify(null!));
    }
}