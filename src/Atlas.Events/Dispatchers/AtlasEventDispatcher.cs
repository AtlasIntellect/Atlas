using Atlas.Events.Interfaces;
using Atlas.Events.Models;

namespace Atlas.Events.Dispatchers;

/// <summary>
/// Represents a dispatcher for publishing events within the Atlas application.
/// </summary>
public sealed class AtlasEventDispatcher : IAtlasEventDispatcher
{
    private readonly Dictionary<Type, List<IAtlasEventHandlerBase>> _handlers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AtlasEventDispatcher"/> class with the specified event handlers.
    /// </summary>
    /// <param name="handlers">The collection of event handlers to register with the dispatcher.</param>
    public AtlasEventDispatcher(
        IEnumerable<IAtlasEventHandlerBase> handlers)
    {
        foreach (var handler in handlers)
        {
            if (!_handlers.TryGetValue(handler.EventType, out var eventHandlers))
            {
                eventHandlers = [];
                _handlers[handler.EventType] = eventHandlers;
            }

            eventHandlers.Add(handler);
        }
    }

    /// <inheritdoc/>
    public async Task PublishAsync(AtlasEvent @event, CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(@event.GetType(), out var eventHandlers))
            return;

        foreach (var handler in eventHandlers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await handler.HandleAsync(
                @event,
                cancellationToken);
        }
    }
}