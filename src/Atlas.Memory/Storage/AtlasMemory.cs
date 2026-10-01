using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Storage;

/// <summary>
/// Provides the Atlas memory capability through the configured memory store.
/// </summary>
/// <param name="store">The configured memory store.</param>
public sealed class AtlasMemory(
    IAtlasMemoryStore store) : IAtlasMemory
{
    /// <inheritdoc/>
    public Task StoreAsync(
        AtlasMemoryEntry memory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);
        
        cancellationToken.ThrowIfCancellationRequested();

        return store.StoreAsync(memory, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<AtlasMemoryEntry?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return store.GetAsync(id, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);

        var terms = query
            .Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (terms.Length == 0)
            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);

        var normalizedQuery =
            string.Join(' ', terms);

        return store.SearchAsync(
            new AtlasMemoryQuery
            {
                Text = normalizedQuery
            },
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        AtlasMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        cancellationToken.ThrowIfCancellationRequested();

        return store.SearchAsync(query, cancellationToken);
    }
}
