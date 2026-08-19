using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Interaction;

/// <summary>
/// Provides the default implementation for processing Atlas interactions.
/// </summary>
public sealed class AtlasInteractionProcessor : IAtlasInteractionProcessor
{
    /// <inheritdoc/>
    public Task<AtlasResponse> ProcessAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = new AtlasResponse
        {
            Content = $"Atlas received: {interaction.Input}"
        };

        return Task.FromResult(response);
    }
}