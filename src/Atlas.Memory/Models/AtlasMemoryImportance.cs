namespace Atlas.Memory.Models;

/// <summary>
/// Represents the importance assigned to an Atlas memory.
/// Numeric values are explicitly assigned for stable persistence.
/// </summary>
public enum AtlasMemoryImportance
{
    /// <summary>
    /// 
    /// </summary>
    Low = 0,

    /// <summary>
    /// 
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 
    /// </summary>
    High = 2,

    /// <summary>
    /// 
    /// </summary>
    Critical = 3
}
