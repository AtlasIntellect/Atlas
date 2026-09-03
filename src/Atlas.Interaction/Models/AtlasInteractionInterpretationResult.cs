namespace Atlas.Interaction.Models;

/// <summary>
/// Represents the result of interpreting an interaction,
/// including confidence and ambiguity information.
/// </summary>
/// <param name="Interpretation"></param>
/// <param name="Confidence"></param>
/// <param name="IsAmbiguous"></param>
public sealed record AtlasInteractionInterpretationResult(
    AtlasInteractionInterpretation Interpretation,
    AtlasInteractionConfidence Confidence,
    bool IsAmbiguous)
{
    /// <summary>
    /// Validates the consistency of confidence and ambiguity information.
    /// </summary>
    public void Validate()
    {
        if (IsAmbiguous &&
            Confidence == AtlasInteractionConfidence.High)
        {
            throw new InvalidOperationException(
                "An ambiguous interpretation cannot have high confidence.");
        }

        if (!IsAmbiguous &&
            Confidence == AtlasInteractionConfidence.Low)
        {
            throw new InvalidOperationException(
                "A non-ambiguous interpretation cannot have low confidence.");
        }
    }
}
