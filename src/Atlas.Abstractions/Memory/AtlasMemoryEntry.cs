namespace Atlas.Abstractions.Memory;

/// <summary>
/// Represents a memory stored by Atlas.
/// </summary>
public sealed class AtlasMemoryEntry
{
    /// <summary>
    /// Gets the unique identifier of the memory.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the content of the memory.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the timestamp when the memory was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}