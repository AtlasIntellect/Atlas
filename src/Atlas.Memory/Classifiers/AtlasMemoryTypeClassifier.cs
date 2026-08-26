using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Classifiers;

/// <summary>
/// Provides the default implementation for classifying memory content.
/// </summary>
public sealed class AtlasMemoryTypeClassifier : IAtlasMemoryTypeClassifier
{
    private static readonly string[] PreferencePrefixes =
    [
        "my favorite ",
        "i prefer ",
        "i like ",
        "i don't like ",
        "i dislike "
    ];

    private static readonly string[] TaskPrefixes =
    [
        "remind me to ",
        "i need to ",
        "i have to ",
        "i must ",
        "don't forget to "
    ];

    /// <inheritdoc/>
    public AtlasMemoryType Classify(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var normalizedContent = content.Trim();

        if (PreferencePrefixes.Any(prefix =>
                normalizedContent.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return AtlasMemoryType.Preference;
        }

        if (TaskPrefixes.Any(prefix =>
                normalizedContent.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return AtlasMemoryType.Task;
        }

        return AtlasMemoryType.Fact;
    }
}