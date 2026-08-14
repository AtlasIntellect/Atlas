namespace Atlas.Abstractions.Events;

/// <summary>
/// Represents an event that is published when the application has started.
/// </summary>
public sealed record ApplicationStartedEvent : AtlasEvent
{
    /// <summary>
    /// Gets the unique identifier of the Atlas instance that published the event.
    /// </summary>
    public required Guid InstanceId { get; init; }
}
