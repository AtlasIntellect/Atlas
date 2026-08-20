using Atlas.Abstractions.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionQueryExtractor"/> class.
/// </summary>
public sealed class AtlasInteractionQueryExtractorTests
{
    /// <summary>
    /// Verifies that an empty input produces an empty query.
    /// </summary>
    [Fact]
    public void ExtractQuery_Should_ReturnEmptyQuery_WhenInputIsEmpty()
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var interaction = new AtlasInteraction
        {
            Input = string.Empty
        };

        var result = extractor.ExtractQuery(interaction);

        Assert.Equal(
            string.Empty,
            result);
    }

    /// <summary>
    /// Verifies that a null interaction is rejected.
    /// </summary>
    [Fact]
    public void ExtractQuery_Should_Throw_WhenInteractionIsNull()
    {
        var extractor = new AtlasInteractionQueryExtractor();

        Assert.Throws<ArgumentNullException>(
            () => extractor.ExtractQuery(null!));
    }

    /// <summary>
    /// Verifies that common question words are removed from the extracted query.
    /// </summary>
    /// <param name="input">The input string containing the query with common question words.</param>
    /// <param name="expectedQuery">The expected query string after removing common question words.</param>
    [Theory]
    [InlineData("What camera did I buy?", "camera")]
    [InlineData("What car do I own?", "car")]
    [InlineData("Which laptop did I buy?", "laptop")]
    public void ExtractQuery_Should_RemoveCommonQuestionWords(
        string input,
        string expectedQuery)
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var interaction = new AtlasInteraction
        {
            Input = input
        };

        var result = extractor.ExtractQuery(interaction);

        Assert.Equal(
            expectedQuery,
            result);
    }

    /// <summary>
    /// Verifies that meaningful words are preserved in the extracted query 
    /// while removing unnecessary or common question words.
    /// </summary>
    [Fact]
    public void ExtractQuery_Should_PreserveMeaningfulWords()
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var interaction = new AtlasInteraction
        {
            Input = "What camera did I buy recently?"
        };

        var result = extractor.ExtractQuery(interaction);

        Assert.Equal(
            "camera recently",
            result);
    }
}