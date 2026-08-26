using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Processors;

/// <summary>
/// Provides the default implementation for processing Atlas interactions.
/// </summary>
public sealed class AtlasInteractionProcessor(
    IAtlasInteractionInterpreter interactionInterpreter,
    IEnumerable<IAtlasInteractionHandler> handlers)
    : IAtlasInteractionProcessor
{
    /// <inheritdoc/>
    public async Task<AtlasResponse> ProcessAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var interpretation =
            interactionInterpreter.Interpret(interaction);

        var handler =
            handlers.FirstOrDefault(
                candidate =>
                    candidate.Intent == interpretation.Intent);

        return handler is null
            ? throw new InvalidOperationException(
                $"No interaction handler registered for intent: {interpretation.Intent}.")
            : await handler.HandleAsync(
                interaction,
                interpretation,
                cancellationToken);
    }
}