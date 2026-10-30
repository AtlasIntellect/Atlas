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

    /// <summary>
    /// Gets the optinal confidence assigned to this memory.
    /// Values must be between 0.0 and 1.0.
    /// Null means confidence has not been assessed.
    /// </summary>
    public double? Confidence
    {
        get;
        init
        {
            if (value.HasValue &&
                (!double.IsFinite(value.Value) ||
                 value.Value < 0.0 ||
                 value.Value > 1.0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Confidence),
                    value,
                    "Memory confidence must be between 0.0 and 1.0");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the optinal provenance describing where this memory originated
    /// </summary>
    public AtlasMemoryProvenance? Provenance { get; init; }
}
