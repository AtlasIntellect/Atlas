namespace Atlas.Memory.Configuration;

/// <summary>
/// Configures Atlas memory storage.
/// </summary>
public sealed class AtlasMemoryOptions
{
    /// <summary>
    /// Gets or sets the configured memory storage mode.
    /// </summary>
    public string StorageMode { get; set; } = "InMemory";
    
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }
}
