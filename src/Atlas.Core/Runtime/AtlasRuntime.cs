using Atlas.Abstractions.Events;
using Atlas.Abstractions.Runtime;

namespace Atlas.Core.Runtime;

/// <inheritdoc/>
public sealed class AtlasRuntime(IAtlasEventDispatcher eventDispatcher) : IAtlasRuntime
{
    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await eventDispatcher.PublishAsync(
            new ApplicationStartedEvent(),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}