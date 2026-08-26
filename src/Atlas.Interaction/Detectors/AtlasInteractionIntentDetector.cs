using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Detectors;

/// <summary>
/// Detects the intent of Atlas interactions.
/// </summary>
public sealed class AtlasInteractionIntentDetector
    : IAtlasInteractionIntentDetector
{
    private static readonly string[] StorePrefixes =
    [
        "remember ",
        "store ",
        "save "
    ];

    private static readonly string[] SearchPrefixes =
    [
        "what ",
        "which ",
        "where ",
        "when ",
        "who ",
        "do you remember ",
        "do you know ",
        "can you tell me ",
        "could you tell me ",
        "tell me "
    ];

    /// <inheritdoc/>
    public AtlasInteractionIntent Detect(
        AtlasInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        var input = interaction.Input.Trim();

        if (IsStoreRequest(input))
        {
            return AtlasInteractionIntent.StoreMemory;
        }

        if (IsSearchRequest(input))
        {
            return AtlasInteractionIntent.SearchMemory;
        }

        return AtlasInteractionIntent.Unknown;
    }

    private static bool IsStoreRequest(
        string input)
    {
        return StorePrefixes.Any(
            prefix =>
                input.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSearchRequest(
        string input)
    {
        if (SearchPrefixes.Any(
                prefix =>
                    input.StartsWith(
                        prefix,
                        StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return input.EndsWith(
            '?');
    }
}