namespace Atlas.Memory.Models;

/// <summary>
/// Describes the origin of a mewmory and optional source-specific metadata.
/// </summary>
public sealed record AtlasMemoryProvenance
{
    /// <summary>
    /// Gets the categroy of the source that produced the memory.
    /// </summary>
    public AtlasMemorySourceType SourceType { get; init; }

    /// <summary>
    /// Gets an optional identifier for the originating source.
    /// For example, a conversation ID or import identifier.
    /// </summary>
    public string? SourceId { get; init; }

    /// <summary>
    /// Gets optinal descriptive information about the source.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets optional source-specific metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}
