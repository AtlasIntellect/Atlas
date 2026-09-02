using Atlas.Memory.Interfaces;

namespace Atlas.Memory.Models;

/// <summary>
/// Represents structured information about an Atlas task.
/// </summary>
public sealed class AtlasTaskData : IAtlasMemoryData
{
    /// <summary>
    /// Gets the actionable description of the task.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public AtlasTaskStatus Status { get; init; } =
        AtlasTaskStatus.Active;

    /// <summary>
    /// Gets the date and time by which the task should be completed,
    /// or <see langword="null"/> when no due date is known.
    /// </summary>
    public DateTimeOffset? DueAt { get; init; }

    /// <summary>
    /// Gets the date and time when the task was completed,
    /// or <see langword="null"/> when the task has not been completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }
}