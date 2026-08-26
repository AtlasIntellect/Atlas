namespace Atlas.Events.Models;

/// <summary>
/// Represents an event that is published when the application is stopping.
/// </summary>
public sealed record ApplicationStoppingEvent : AtlasEvent
{
    /// <summary>
    /// Gets the unique identifier of the Atlas instance that published when the application is stopping.
    /// </summary>
    public required Guid InstanceId { get; init; }
}
