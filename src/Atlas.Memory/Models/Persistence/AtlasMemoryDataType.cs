namespace Atlas.Memory.Models.Persistence;

/// <summary>
/// Specifies the concrete type of structured data associated with a memory.
/// </summary>
public enum AtlasMemoryDataType
{
    /// <summary>
    /// Indicates that the memory has no structured interpretation data.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that the memory contains task data.
    /// </summary>
    Task = 1
}
