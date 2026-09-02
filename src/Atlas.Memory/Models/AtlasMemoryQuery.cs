namespace Atlas.Memory.Models;

/// <summary>
/// Represents optional criteria used when searching Atlas memories.
/// </summary>
public sealed record AtlasMemoryQuery
{
    /// <summary>
    /// Gets the text used to match memory content.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Gets the memory type to include.
    /// </summary>
    public AtlasMemoryType? Type { get; init; }
}