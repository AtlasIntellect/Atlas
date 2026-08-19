using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Commands;

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