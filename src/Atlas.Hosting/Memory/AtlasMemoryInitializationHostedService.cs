using Atlas.Memory.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Atlas.Hosting.Memory;

/// <summary>
/// Initializes the Atlas memory store during application startup.
/// </summary>
/// <param name="initializer">
/// The memory store initializer selected by the configured storage provider.
/// </param>
public sealed class AtlasMemoryInitializationHostedService(
    IAtlasMemoryStoreInitializer initializer) : IHostedService
{
    /// <summary>
    /// Initializes the memory store before other Atlas hosted services start.
    /// </summary>
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        return initializer.InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// Performs no shutdown work because memory store initialization
    /// does not own any application lifetime resources.
    /// </summary>
    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
