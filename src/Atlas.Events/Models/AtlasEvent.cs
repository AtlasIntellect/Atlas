namespace Atlas.Events.Models;

/// <summary>
/// Represents an event that can be published and subscribed to within the Atlas framework.
/// </summary>
public abstract record AtlasEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp indicating when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}