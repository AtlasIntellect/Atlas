using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;

namespace Atlas.Core.Interaction;

/// <summary>
/// Provides the default implementation for processing Atlas interactions.
/// </summary>
public sealed class AtlasInteractionProcessor(
    IAtlasCommandDispatcher commandDispatcher,
    IAtlasInteractionQueryExtractor queryExtractor)
    : IAtlasInteractionProcessor
{
    /// <inheritdoc/>
    public async Task<AtlasResponse> ProcessAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var intent =
            AtlasInteractionIntentDetector.Detect(interaction);

        switch (intent)
        {
            case AtlasInteractionIntent.SearchMemory:
            {
                var query = queryExtractor.ExtractQuery(interaction);

                var memories =
                await commandDispatcher.DispatchAsync<
                    SearchMemoryCommand,
                    IReadOnlyList<AtlasMemoryEntry>>(
                    new SearchMemoryCommand(query),
                    cancellationToken);

                var content = memories.Count == 0
                    ? "I couldn't find any matching memories."
                    : string.Join(
                        Environment.NewLine,
                        memories.Select(memory => memory.Content));

                return new AtlasResponse
                {
                    Content = content
                };
            }

            case AtlasInteractionIntent.StoreMemory:
            {
                await commandDispatcher.DispatchAsync<StoreMemoryCommand, AtlasMemoryEntry>(
                    new StoreMemoryCommand(interaction.Input),
                    cancellationToken);

                return new AtlasResponse
                {
                    Content = "Memory stored successfully."
                };
            }

            default:
                return new AtlasResponse
                {
                    Content = $"Atlas received: {interaction.Input}"
                };
        }
    }
}