using Atlas.Abstractions.Events;

namespace Atlas.Core.Events;

/// <summary>
/// Represents a dispatcher for publishing events within the Atlas application.
/// </summary>
public sealed class AtlasEventDispatcher : IAtlasEventDispatcher
{
    /// <inheritdoc/>
    public Task PublishAsync(AtlasEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}