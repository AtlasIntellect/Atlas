using Atlas.Abstractions.Runtime;
using Microsoft.Extensions.Hosting;

namespace Atlas.Hosting.Runtime;

/// <summary>
/// Represents a hosted service that manages the lifecycle of the <see cref="IAtlasRuntime"/>.
/// </summary>
/// <param name="runtime"></param>
public sealed class AtlasRuntimeHostedService(
    IAtlasRuntime runtime) : IHostedService
{
    /// <summary>
    /// Starts the <see cref="IAtlasRuntime"/> when the hosted service starts.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return runtime.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the <see cref="IAtlasRuntime"/> when the hosted service stops.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return runtime.StopAsync(cancellationToken);
    }
}