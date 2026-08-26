using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interpretators;

/// <summary>
/// Provides functionality to interpret <see cref="AtlasInteraction"/> instances into structured representations
/// by detecting their intent and optionally extracting associated queries.
/// </summary>
/// <param name="intentDetector">The intent detector used to analyze the interaction's intent.</param>
/// <param name="queryExtractor">The query extractor used to extract query data from the interaction.</param>
/// <param name="memoryContentExtractor">The memory content extractor used to extract the memory content from the interaction.</param>
public sealed class AtlasInteractionInterpreter(
    IAtlasInteractionIntentDetector intentDetector,
    IAtlasInteractionQueryExtractor queryExtractor,
    IAtlasInteractionMemoryContentExtractor memoryContentExtractor)
    : IAtlasInteractionInterpreter
{
    /// <inheritdoc/>
    public AtlasInteractionInterpretation Interpret(
        AtlasInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

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

        return new AtlasInteractionInterpretation(
            intent,
            query,
            memoryContent);
    }
}