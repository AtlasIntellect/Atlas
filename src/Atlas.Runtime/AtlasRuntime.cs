using Atlas.Events.Interfaces;
using Atlas.Events.Models;
using Atlas.Runtime.Interfaces;

namespace Atlas.Runtime;

/// <inheritdoc/>
public sealed class AtlasRuntime(
    IAtlasEventDispatcher eventDispatcher,
    IAtlasApplicationContext applicationContext) : IAtlasRuntime
{
    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await eventDispatcher.PublishAsync(
            new ApplicationStartedEvent
            {
                InstanceId = applicationContext.InstanceId
            },
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await eventDispatcher.PublishAsync(
            new ApplicationStoppingEvent
            {
                InstanceId = applicationContext.InstanceId
            },
            cancellationToken);
    }
}