namespace Atlas.Memory.Models;

/// <summary>
/// Represents the lifecycle state of an Atlas memory.
/// </summary>
public enum AtlasMemoryLifecycleState
{
    /// <summary>
    /// The memory is active and participates in normal memory operations.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The memory has been archived and is not active by default.
    /// </summary>
    Archived = 1
}
