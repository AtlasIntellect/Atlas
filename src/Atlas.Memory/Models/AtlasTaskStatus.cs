namespace Atlas.Memory.Models;

/// <summary>
/// Represents the current status of an Atlas task.
/// </summary>
public enum AtlasTaskStatus
{
    /// <summary>
    /// The task is currently active and has not been completed or cancelled.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The task has been completed.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The task has been cancelled and is no longer active.
    /// </summary>
    Cancelled = 2
}
