using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Interpreters;

/// <summary>
/// Provides the default implementation for interpreting memory content.
/// </summary>
public sealed class AtlasMemoryInterpreter : IAtlasMemoryInterpreter
{
    private static readonly string[] TaskPrefixes =
    [
        "i need to ",
        "i have to ",
        "i must ",
        "don't forget to ",
        "remind me to "
    ];

    /// <inheritdoc/>
    public IAtlasMemoryData? Interpret(
        string content,
        AtlasMemoryType type)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (type != AtlasMemoryType.Task)
            return null;

        var normalizedContent = content.Trim();

        foreach (var prefix in TaskPrefixes)
        {
            if (!normalizedContent.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var description =
                normalizedContent[prefix.Length..].Trim();

            if (string.IsNullOrWhiteSpace(description))
                return null;

            description =
                char.ToUpperInvariant(description[0])
                + description[1..];

            return new AtlasTaskData
            {
                Description = description
            };
        }

        return null;
    }
}