using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Formatters;

/// <summary>
/// Provides the default implementation for formatting memory search results.
/// </summary>
public sealed class AtlasMemorySearchResponseFormatter
    : IAtlasMemorySearchResponseFormatter
{
    /// <inheritdoc/>
    public AtlasResponse Format(
        IReadOnlyList<AtlasMemoryEntry> memories)
    {
        if (memories.Count == 0)
        {
            return new AtlasResponse
            {
                Content = "I couldn't find any matching memories."
            };
        }

        return new AtlasResponse
        {
            Content = string.Join(
                Environment.NewLine,
                memories.Select(memory => memory.Content))
        };
    }
}