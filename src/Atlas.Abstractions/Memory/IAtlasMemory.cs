namespace Atlas.Abstractions.Memory;

/// <summary>
/// Provides access to Atlas memory.
/// </summary>
public interface IAtlasMemory
{
    /// <summary>
    /// Stores the specified memory.
    /// </summary>
    /// <param name="memory">The memory to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAsync(
        AtlasMemoryEntry memory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a memory by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the memory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation and containing the memory,
    /// or <see langword="null"/> when no memory exists with the specified identifier.
    /// </returns>
    Task<AtlasMemoryEntry?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}