using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Interaction;

/// <summary>
/// Extracts the actual memory content from Atlas interactions.
/// </summary>
public sealed class AtlasInteractionMemoryContentExtractor
    : IAtlasInteractionMemoryContentExtractor
{
    private static readonly string[] Prefixes =
    [
        "remember that ",
        "remember ",
        "store that ",
        "save that "
    ];

    /// <inheritdoc/>
    public string ExtractContent(AtlasInteraction interaction)
    {
        var input = interaction.Input.Trim();
        
        var matchingPrefix = Prefixes
            .FirstOrDefault(prefix =>
                input.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase));
        
        return matchingPrefix != null ?
            input[matchingPrefix.Length..].Trim()
            : input;
    }
}