namespace Atlas.Abstractions.Events;

/// <summary>
/// Represents a dispatcher for publishing events within the Atlas framework.
/// </summary>
public interface IAtlasEventHandler<in TEvent> : IAtlasEventHandlerBase where TEvent : AtlasEvent
{
    /// <inheritdoc />
    Type IAtlasEventHandlerBase.EventType => typeof(TEvent);

    /// <inheritdoc />
    Task IAtlasEventHandlerBase.HandleAsync(
        AtlasEvent @event,
        CancellationToken cancellationToken)
    {
        return HandleAsync((TEvent)@event, cancellationToken);
    }

    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);
}