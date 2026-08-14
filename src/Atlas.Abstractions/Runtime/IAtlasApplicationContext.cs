namespace Atlas.Abstractions.Runtime;

/// <summary>
/// Represents the current application context of an Atlas instance.
/// </summary>
public interface IAtlasApplicationContext
{
    /// <summary>
    /// Gets the configuration name of the Atlas instance.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the unique identifier of this Atlas instance.
    /// </summary>
    Guid InstanceId { get; }
}