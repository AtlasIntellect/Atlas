using Atlas.Abstractions.Memory;

namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Defines a contract for formatting memory search results into Atlas responses.
/// </summary>
public interface IAtlasMemorySearchResponseFormatter
{
    /// <summary>
    /// Formats the specified memory search results into an Atlas response.
    /// </summary>
    /// <param name="memories">The memories returned by the search.</param>
    /// <returns>An Atlas response containing the formatted search results.</returns>
    AtlasResponse Format(
        IReadOnlyList<AtlasMemoryEntry> memories);
}