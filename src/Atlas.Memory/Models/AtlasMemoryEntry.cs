namespace Atlas.Memory.Models;

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

    /// <summary>
    /// Gets the timestamp when the memory was last updated, if applicable.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Gets the type of the memory.
    /// </summary>
    public AtlasMemoryType Type { get; init; } = AtlasMemoryType.Fact;

    /// <summary>
    /// Gets the lifecycle state of the memory.
    /// </summary>
    public AtlasMemoryLifecycleState LifecycleState { get; init; } = AtlasMemoryLifecycleState.Active;

    /// <summary>
    /// Gets the structured interpretation of the memory, when available.
    /// </summary>
    public AtlasMemoryInterpretation? Interpretation { get; init; }
}
