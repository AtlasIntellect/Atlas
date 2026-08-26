namespace Atlas.Commands.Models;

/// <summary>
/// Represents information about the current Atlas instance.
/// </summary>
public sealed class AtlasInfo
{
    /// <summary>
    /// Gets the name of the Atlas instance.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the unique identifier of the Atlas instance.
    /// </summary>
    public required Guid InstanceId { get; init; }
}