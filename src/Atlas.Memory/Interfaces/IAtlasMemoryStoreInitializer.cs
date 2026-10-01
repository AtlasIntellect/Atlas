namespace Atlas.Memory.Interfaces;

/// <summary>
/// Initializes the persistent Atlas memory store.
/// </summary>
public interface IAtlasMemoryStoreInitializer
{
    /// <summary>
    /// Initializes the memory store and applies any pending schema migrations.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitializeAsync(
        CancellationToken cancellationToken = default);
}
