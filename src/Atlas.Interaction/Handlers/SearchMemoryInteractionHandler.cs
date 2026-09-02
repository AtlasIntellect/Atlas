using Atlas.Commands.Interfaces;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;
using Atlas.Memory.Commands;
using Atlas.Memory.Models;

namespace Atlas.Interaction.Handlers;

/// <summary>
/// Handles interactions that request a memory search.
/// </summary>
public sealed class SearchMemoryInteractionHandler(
    IAtlasCommandDispatcher commandDispatcher)
    : IAtlasInteractionHandler
{
    /// <inheritdoc/>
    public AtlasInteractionIntent Intent =>
        AtlasInteractionIntent.SearchMemory;

    /// <inheritdoc/>
    public async Task<AtlasResponse> HandleAsync(
        AtlasInteraction interaction,
        AtlasInteractionInterpretation interpretation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = interpretation.Query ?? throw new InvalidOperationException(
                "Search-memory interpretation did not contain a query.");

        var memories =
            await commandDispatcher.DispatchAsync<
                SearchMemoryCommand,
                IReadOnlyList<AtlasMemoryEntry>>(
                new SearchMemoryCommand(query),
                cancellationToken);

        return new AtlasResponse
        {
            Content = memories.Count == 0
                ? "I couldn't find any matching memories."
                : string.Join(
                    Environment.NewLine,
                    memories.Select(memory => memory.Content))
        };
    }
}