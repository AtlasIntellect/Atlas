using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;

namespace Atlas.Core.Interaction;

/// <summary>
/// Provides the default implementation for processing Atlas interactions.
/// </summary>
public sealed class AtlasInteractionProcessor(
    IAtlasCommandDispatcher commandDispatcher)
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
                await commandDispatcher.DispatchAsync<
                    SearchMemoryCommand,
                    IReadOnlyList<AtlasMemoryEntry>>(
                    new SearchMemoryCommand(interaction.Input),
                    cancellationToken);

                return new AtlasResponse
                {
                    Content = "Atlas detected a memory search."
                };

            default:
                return new AtlasResponse
                {
                    Content = $"Atlas received: {interaction.Input}"
                };
        }
    }
}