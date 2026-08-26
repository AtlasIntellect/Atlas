namespace Atlas.Memory.Models;

/// <summary>
/// Represents the current status of an Atlas task.
/// </summary>
public enum AtlasTaskStatus
{
    /// <summary>
    /// The task is currently active and has not been completed or cancelled.
    /// </summary>
    Active,

    /// <summary>
    /// The task has been completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The task has been cancelled and is no longer active.
    /// </summary>
    Cancelled
}