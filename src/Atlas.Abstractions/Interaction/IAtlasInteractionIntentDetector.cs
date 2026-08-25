namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Detects the intent of an Atlas interaction.
/// </summary>
public interface IAtlasInteractionIntentDetector
{
    /// <summary>
    /// Detects the intent of the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to analyze.</param>
    /// <returns>The detected interaction intent.</returns>
    AtlasInteractionIntent Detect(
        AtlasInteraction interaction);
}