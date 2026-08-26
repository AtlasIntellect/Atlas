using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interfaces;

/// <summary>
/// Defines a handler for processing an Atlas interaction for a specific intent.
/// </summary>
public interface IAtlasInteractionHandler
{
    /// <summary>
    /// Gets the interaction intent handled by this handler.
    /// </summary>
    AtlasInteractionIntent Intent { get; }

    /// <summary>
    /// Handles the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to handle.</param>
    /// <param name="interpretation">The interpretation of the interaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response produced by the handler.</returns>
    Task<AtlasResponse> HandleAsync(
        AtlasInteraction interaction,
        AtlasInteractionInterpretation interpretation,
        CancellationToken cancellationToken = default);
}