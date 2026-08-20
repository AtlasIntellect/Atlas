using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Interaction;

/// <summary>
/// Detects the intent of Atlas interactions.
/// </summary>
public static class AtlasInteractionIntentDetector
{
    /// <summary>
    /// Detects the intent of the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to analyze.</param>
    /// <returns>The detected interaction intent.</returns>
    public static AtlasInteractionIntent Detect(
        AtlasInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        var input = interaction.Input.Trim();

        if (input.Contains("camera", StringComparison.OrdinalIgnoreCase))
        {
            return AtlasInteractionIntent.SearchMemory;
        }

        return AtlasInteractionIntent.Unknown;
    }
}