namespace Atlas.Memory.Models;

/// <summary>
/// Identifies the source categroy from which a memory originated.
/// </summary>
public enum AtlasMemorySourceType
{
    /// <summary>
    /// 
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 
    /// </summary>
    UserProvided = 1,

    /// <summary>
    /// 
    /// </summary>
    Conversation = 2,

    /// <summary>
    /// 
    /// </summary>
    Tool = 3,

    /// <summary>
    /// 
    /// </summary>
    Imported = 4,

    /// <summary>
    /// 
    /// </summary>
    SystemGenerated = 5
}
