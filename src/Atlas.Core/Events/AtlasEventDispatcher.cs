using Atlas.Abstractions.Events;

namespace Atlas.Core.Events;

/// <summary>
/// Represents a dispatcher for publishing events within the Atlas application.
/// </summary>
public sealed class AtlasEventDispatcher : IAtlasEventDispatcher
{
    private readonly Dictionary<Type, List<Func<AtlasEvent, CancellationToken, Task>>> _handlers = [];

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="handler">The event handler to register.</param>
    public void RegisterHandler<TEvent>(IAtlasEventHandler<TEvent> handler) where TEvent : AtlasEvent
    {
        var eventType = typeof(TEvent);

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            _handlers[eventType] = handlers;
        }

        handlers.Add((@event, cancellationToken) =>
            handler.HandleAsync((TEvent)@event, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task PublishAsync(AtlasEvent @event, CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(@event.GetType(), out var handlers))
            return;

        foreach (var handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await handler(@event, cancellationToken);
        }
    }
}