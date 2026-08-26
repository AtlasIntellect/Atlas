namespace Atlas.Abstractions.Runtime;

/// <summary>
/// Defines the interface for the Atlas runtime, which provides core functionality and services for the Atlas framework.
/// </summary>
public interface IAtlasRuntime
{
    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the Atlas runtime and releases any resources used by it.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}