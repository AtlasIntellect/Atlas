namespace Atlas.Memory.Models;

/// <summary>
/// Represents a memory result returned by a memory search.
/// </summary>
/// <param name="Content">The memory content.</param>
public sealed record AtlasMemorySearchResult(
    string Content);