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

    /// <inheritdoc/>
    public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);

        var normalizedQuery = string.Join(
            ' ',
            query.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));

        var results = _memories.Values
            .Where(memory =>
                memory.Content.Contains(
                    normalizedQuery,
                    StringComparison.OrdinalIgnoreCase))
            .Select(memory => new
            {
                Memory = memory,
                Score = CalculateRelevance(
                    memory.Content,
                    normalizedQuery)
            })
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Memory.CreatedAt)
            .Select(result => result.Memory)
            .ToList();

        return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(results);
    }

    private static int CalculateRelevance(
        string content,
        string query)
    {
        if (string.Equals(
                content,
                query,
                StringComparison.OrdinalIgnoreCase))
        {
            return int.MaxValue;
        }

        var count = 0;
        var startIndex = 0;

        while (true)
        {
            var index = content.IndexOf(
                query,
                startIndex,
                StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                break;

            count++;
            startIndex = index + query.Length;
        }

        return count;
    }
}