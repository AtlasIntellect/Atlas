namespace Atlas.Abstractions.Events;

/// <summary>
/// Represents an event that is published when the application has started.
/// </summary>
public sealed record ApplicationStartedEvent : AtlasEvent;
