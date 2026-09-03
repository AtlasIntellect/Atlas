using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Models;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionInterpretationResult"/>.
/// </summary>
public sealed class AtlasInteractionInterpretationResultTests
{
    /// <summary>
    /// Validates that a high-confidence, non-ambiguous result is processed successfully
    /// without throwing exceptions.
    /// </summary>
    [Fact]
    public void Validate_Should_Succeed_ForHighConfidenceNonAmbiguousResult()
    {
        var result = CreateResult(AtlasInteractionConfidence.High, false);

        result.Validate();
    }

    /// <summary>
    /// Validates that a medium-confidence, ambiguous result is processed successfully
    /// without throwing exceptions.
    /// </summary>
    [Fact]
    public void Validate_Should_Succeed_ForMediumConfidenceAmbiguousResult()
    {
        var result = CreateResult(AtlasInteractionConfidence.Medium, true);

        result.Validate();
    }

    /// <summary>
    /// Validates that a low-confidence, ambiguous result is processed successfully
    /// without throwing exceptions.
    /// </summary>
    [Fact]
    public void Validate_Should_Succeed_ForLowConfidenceAmbiguousResult()
    {
        var result = CreateResult(AtlasInteractionConfidence.Low, true);

        result.Validate();
    }

    /// <summary>
    /// Validates that a high-confidence, ambiguous result throws an 
    /// <see cref="InvalidOperationException"/> with the expected message.
    /// </summary>
    [Fact]
    public void Validate_Should_Throw_ForHighConfidenceAmbiguousResult()
    {
        var result = CreateResult(AtlasInteractionConfidence.High, true);

        var exception =
            Assert.Throws<InvalidOperationException>(
                result.Validate);

        Assert.Equal(
            "An ambiguous interpretation cannot have high confidence.",
            exception.Message);
    }

    /// <summary>
    /// Validates that a low-confidence, non-ambiguous result throws an 
    /// <see cref="InvalidOperationException"/> with the expected message.
    /// </summary>
    [Fact]
    public void Validate_Should_Throw_ForLowConfidenceNonAmbiguousResult()
    {
        var result = CreateResult(AtlasInteractionConfidence.Low, false);

        var exception =
            Assert.Throws<InvalidOperationException>(
                result.Validate);

        Assert.Equal(
            "A non-ambiguous interpretation cannot have low confidence.",
            exception.Message);
    }

    private static AtlasInteractionInterpretationResult CreateResult(
        AtlasInteractionConfidence confidence,
        bool isAmbiguous)
    {
        return new AtlasInteractionInterpretationResult(
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.SearchMemory,
                "camera",
                null),
            confidence,
            isAmbiguous);
    }
}