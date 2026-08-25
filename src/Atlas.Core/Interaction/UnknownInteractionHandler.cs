using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Interaction;

/// <summary>
/// Handles interactions for which Atlas cannot determine a supported intent.
/// </summary>
public sealed class UnknownInteractionHandler : IAtlasInteractionHandler
{
    /// <inheritdoc/>
    public AtlasInteractionIntent Intent =>
        AtlasInteractionIntent.Unknown;

    /// <inheritdoc/>
    public Task<AtlasResponse> HandleAsync(
        AtlasInteraction interaction,
        AtlasInteractionInterpretation interpretation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            new AtlasResponse
            {
                Content = $"Atlas received: {interaction.Input}"
            });
    }
}
