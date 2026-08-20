using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Interaction;

/// <summary>
/// Provides the default implementation for processing Atlas interactions.
/// </summary>
public sealed class AtlasInteractionProcessor(
    IEnumerable<IAtlasInteractionHandler> handlers)
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

        var handler =
            handlers.FirstOrDefault(
                candidate => candidate.Intent == intent);

        if (handler is null)
            throw new InvalidOperationException(
                $"No interaction handler registered for intent: {intent}.");

        return await handler.HandleAsync(
            interaction,
            cancellationToken);
    }
}
