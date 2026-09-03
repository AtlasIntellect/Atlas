namespace Atlas.Interaction.Models;

/// <summary>
/// Represents the structured interpretation produced by a language model
/// for an Atlas interaction.
/// </summary>
/// <param name="Intent">The interpreted interaction intent.</param>
/// <param name="Query">The optional memory search query.</param>
/// <param name="MemoryContent">The optional memory content to store.</param>
/// <param name="Confidence">The confidence level of the interpretation.</param>
/// <param name="IsAmbiguous">Indicates whether the interaction is ambiguous.</param>
public sealed record AtlasStructuredInteractionInterpretation(
    AtlasInteractionIntent Intent,
    string? Query,
    string? MemoryContent,
    AtlasInteractionConfidence Confidence,
    bool IsAmbiguous)
{
    /// <summary>
    /// Validates that the interpretation contains consistent information.
    /// </summary>
    public void Validate()
    {
        switch (Intent)
        {
            case AtlasInteractionIntent.SearchMemory:
                if (string.IsNullOrWhiteSpace(Query))
                {
                    throw new InvalidOperationException(
                        "SearchMemory interpretations require a query.");
                }

                if (MemoryContent is not null)
                {
                    throw new InvalidOperationException(
                        "SearchMemory interpretations must not contain memory content.");
                }

                break;

            case AtlasInteractionIntent.StoreMemory:
                if (string.IsNullOrWhiteSpace(MemoryContent))
                {
                    throw new InvalidOperationException(
                        "StoreMemory interpretations require memory content.");
                }

                if (Query is not null)
                {
                    throw new InvalidOperationException(
                        "StoreMemory interpretations must not contain a query.");
                }

                break;

            case AtlasInteractionIntent.Unknown:
                if (Query is not null)
                {
                    throw new InvalidOperationException(
                        "Unknown interpretations must not contain a query.");
                }

                if (MemoryContent is not null)
                {
                    throw new InvalidOperationException(
                        "Unknown interpretations must not contain memory content.");
                }

                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported interaction intent: {Intent}.");
        }

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
