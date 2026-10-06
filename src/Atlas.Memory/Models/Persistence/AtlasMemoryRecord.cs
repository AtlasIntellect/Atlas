namespace Atlas.Memory.Models.Persistence;

/// <summary>
/// Represents the persisted representation of an Atlas memory.
/// </summary>
public sealed class AtlasMemoryRecord
{
    /// <summary>
    /// Gets or sets the unique identifier of the memory.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the content of the memory.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the memory was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the memory was last updated, if applicable.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Gets or sets the type of the memory.
    /// </summary>
    public AtlasMemoryType Type { get; set; }

    /// <summary>
    /// Gets or sets the type of structured interpretation data.
    /// </summary>
    public AtlasMemoryDataType? InterpretationType { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle state of the memory.
    /// </summary>
    public AtlasMemoryLifecycleState LifecycleState { get; init; } = AtlasMemoryLifecycleState.Active;

    /// <summary>
    /// Gets or sets the serialized structured interpretation data.
    /// </summary>
    public string? InterpretationData { get; set; }
}
