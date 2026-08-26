using Atlas.Events.Models;

namespace Atlas.Events.Interfaces;

/// <summary>
/// Represents a dispatcher for publishing events within the Atlas framework.
/// </summary>
public interface IAtlasEventDispatcher
{
    /// <summary>
    /// Publishes the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(AtlasEvent @event, CancellationToken cancellationToken = default);
}