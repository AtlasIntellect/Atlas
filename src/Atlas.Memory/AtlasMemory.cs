using Atlas.Abstractions.Memory;

namespace Atlas.Memory;

/// <summary>
/// Provides an in-memory implementation of <see cref="IAtlasMemory"/>.
/// </summary>
public sealed class AtlasMemory : IAtlasMemory
{
    private readonly Dictionary<Guid, AtlasMemoryEntry> _memories = [];

    /// <inheritdoc/>
    public Task StoreAsync(
        AtlasMemoryEntry memory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _memories[memory.Id] = memory;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<AtlasMemoryEntry?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _memories.TryGetValue(id, out var memory);

        return Task.FromResult(memory);
    }
}