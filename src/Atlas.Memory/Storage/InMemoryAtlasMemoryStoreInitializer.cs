using Atlas.Memory.Interfaces;

namespace Atlas.Memory.Storage;

/// <summary>
/// Provides initialization for the in-memory Atlas memory store.
/// </summary>
public sealed class InMemoryAtlasMemoryStoreInitializer 
    : IAtlasMemoryStoreInitializer
{
    /// <inheritdoc />
    public Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
