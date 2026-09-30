namespace Atlas.Memory.Models;

/// <summary>
/// Specifies the storage implementation used for Atlas memories
/// </summary>
public enum AtlasMemoryStorageMode
{
    /// <summary>
    /// Stores memories only in process memory.
    /// </summary>
    InMemory = 0,

    /// <summary>
    /// Stores memories in a SQLite database.
    /// </summary>
    Sqlite = 1
}
