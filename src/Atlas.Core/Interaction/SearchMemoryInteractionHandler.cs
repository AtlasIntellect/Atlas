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
    IAtlasMemorySearchResponseFormatter responseFormatter)
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

        return responseFormatter.Format(memories);
    }
}