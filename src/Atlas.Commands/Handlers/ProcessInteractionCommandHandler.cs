using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Commands.Handlers;

/// <summary>
/// Handles commands to process interactions with Atlas.
/// </summary>
/// <param name="processor">The Atlas interaction processor.</param>
public sealed class ProcessInteractionCommandHandler(
    IAtlasInteractionProcessor processor)
    : IAtlasCommandHandler<
        ProcessInteractionCommand,
        AtlasResponse>
{
    /// <inheritdoc/>
    public Task<AtlasResponse> HandleAsync(
        ProcessInteractionCommand command,
        CancellationToken cancellationToken = default)
    {
        return processor.ProcessAsync(
            command.Interaction,
            cancellationToken);
    }
}