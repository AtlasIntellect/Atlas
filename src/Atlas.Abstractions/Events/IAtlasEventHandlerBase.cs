namespace Atlas.Abstractions.Events;

/// <summary>
/// Represents the non-generic contract for an Atlas event handler.
/// </summary>
public interface IAtlasEventHandlerBase
{
    /// <summary>
    /// Gets the type of event handler by this handler.
    /// </summary>
    Type EventType { get; }

    /// <summary>
    /// Handles an Atlas event asynchronously.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(
        AtlasEvent @event,
        CancellationToken cancellationToken = default);
}