using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interfaces;

/// <summary>
/// Interprets Atlas interactions into structured representations.
/// </summary>
public interface IAtlasInteractionInterpreter
{
    /// <summary>
    /// Interprets the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to interpret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The interpretation of the interaction.</returns>
    Task<AtlasInteractionInterpretationResult> InterpretAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default);
}