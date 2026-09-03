using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interpreters;

/// <summary>
/// Provides functionality to interpret <see cref="AtlasInteraction"/> instances
/// by detecting their intent and optionally extracting associated queries or memory content.
/// </summary>
/// <param name="intentDetector">The intent detector used to analyze the interaction's intent.</param>
/// <param name="queryExtractor">The query extractor used to extract query data from the interaction.</param>
/// <param name="memoryContentExtractor">The memory content extractor used to extract memory content from the interaction.</param>
public sealed class AtlasInteractionInterpreter(
    IAtlasInteractionIntentDetector intentDetector,
    IAtlasInteractionQueryExtractor queryExtractor,
    IAtlasInteractionMemoryContentExtractor memoryContentExtractor)
    : IAtlasInteractionInterpreter
{
    /// <inheritdoc/>
    public Task<AtlasInteractionInterpretationResult> InterpretAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        cancellationToken.ThrowIfCancellationRequested();

        var intent =
            intentDetector.Detect(interaction);

        var query =
            intent == AtlasInteractionIntent.SearchMemory
                ? queryExtractor.ExtractQuery(interaction)
                : null;

        var memoryContent =
            intent == AtlasInteractionIntent.StoreMemory
                ? memoryContentExtractor.ExtractContent(interaction)
                : null;

        var interpretation =
            new AtlasInteractionInterpretation(
                intent,
                query,
                memoryContent);

        var confidence =
            intent == AtlasInteractionIntent.Unknown
                ? AtlasInteractionConfidence.Low
                : AtlasInteractionConfidence.High;

        var isAmbiguous =
            intent == AtlasInteractionIntent.Unknown;

        return Task.FromResult(
            new AtlasInteractionInterpretationResult(
                interpretation,
                confidence,
                isAmbiguous));
    }
}
