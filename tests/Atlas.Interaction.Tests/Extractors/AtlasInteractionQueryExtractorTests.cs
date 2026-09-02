using Atlas.Interaction.Extractors;
using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Extractors;

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

    /// <summary>
    /// Verifies that ownership questions are converted into useful search queries.
    /// </summary>
    /// <param name="input">The natural-language question.</param>
    /// <param name="expectedQuery">The expected search query.</param>
    [Theory]
    [InlineData("What camera do I have?", "camera")]
    [InlineData("What camera do I own?", "camera")]
    [InlineData("Which camera do I have?", "camera")]
    [InlineData("Which camera do I own?", "camera")]
    public void ExtractQuery_Should_HandleOwnershipQuestions(
        string input,
        string expectedQuery)
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var result = extractor.ExtractQuery(
            new AtlasInteraction
            {
                Input = input
            });

        Assert.Equal(
            expectedQuery,
            result);
    }

    /// <summary>
    /// Verifies that non-question input is preserved as a search query.
    /// </summary>
    /// <param name="input">The input statement.</param>
    [Theory]
    [InlineData("Canon camera")]
    [InlineData("my Canon camera")]
    [InlineData("camera for photography")]
    public void ExtractQuery_Should_PreserveNonQuestionInput(
        string input)
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var result = extractor.ExtractQuery(
            new AtlasInteraction
            {
                Input = input
            });

        Assert.Equal(
            input,
            result);
    }

    /// <summary>
    /// Verifies that surrounding whitespace and trailing punctuation are removed
    /// from an extracted query.
    /// </summary>
    [Fact]
    public void ExtractQuery_Should_NormalizeWhitespaceAndPunctuation()
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var whitespaceResult = extractor.ExtractQuery(
            new AtlasInteraction
            {
                Input = "  Canon camera  "
            });

        var punctuationResult = extractor.ExtractQuery(
            new AtlasInteraction
            {
                Input = "Canon camera?!"
            });

        Assert.Equal(
            "Canon camera",
            whitespaceResult);

        Assert.Equal(
            "Canon camera",
            punctuationResult);
    }

    /// <summary>
    /// Verifies that conversational prefixes are removed before extracting the query.
    /// </summary>
    /// <param name="input">The natural-language question.</param>
    /// <param name="expectedQuery">The expected search query.</param>
    [Theory]
    [InlineData(
        "Can you tell me what camera did I buy?",
        "camera")]
    [InlineData(
        "Could you tell me what laptop do I own?",
        "laptop")]
    [InlineData(
        "Tell me what phone do I have?",
        "phone")]
    public void ExtractQuery_Should_RemoveConversationalPrefix(
        string input,
        string expectedQuery)
    {
        var extractor = new AtlasInteractionQueryExtractor();

        var interaction = new AtlasInteraction
        {
            Input = input
        };

        var result = extractor.ExtractQuery(interaction);

        Assert.NotNull(result);
        Assert.Equal(
            expectedQuery,
            result);
    }
}