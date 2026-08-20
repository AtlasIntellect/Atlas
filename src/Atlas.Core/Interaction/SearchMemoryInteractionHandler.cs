using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;

namespace Atlas.Core.Interaction;

/// <summary>
/// Handles interactions that request a memory search.
/// </summary>
public sealed class SearchMemoryInteractionHandler(
    IAtlasCommandDispatcher commandDispatcher,
    IAtlasInteractionQueryExtractor queryExtractor)
    : IAtlasInteractionHandler
{
    /// <inheritdoc/>
    public AtlasInteractionIntent Intent =>
        AtlasInteractionIntent.SearchMemory;

    /// <inheritdoc/>
    public async Task<AtlasResponse> HandleAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query =
            queryExtractor.ExtractQuery(interaction);

        var memories =
            await commandDispatcher.DispatchAsync<
                SearchMemoryCommand,
                IReadOnlyList<AtlasMemoryEntry>>(
                new SearchMemoryCommand(query),
                cancellationToken);

        if (memories.Count == 0)
        {
            return new AtlasResponse
            {
                Content = "I couldn't find any matching memories."
            };
        }

        return new AtlasResponse
        {
            Content = string.Join(
                Environment.NewLine,
                memories.Select(memory => memory.Content))
        };
    }
}