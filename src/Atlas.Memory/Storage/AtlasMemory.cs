using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Storage;

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

        var terms = query
            .Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (terms.Length == 0)
            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);

        var normalizedQuery = string.Join(
            ' ',
            terms);

        return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(
            SearchMemories(
                normalizedQuery,
                terms,
                null));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        AtlasMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        cancellationToken.ThrowIfCancellationRequested();

        var terms = string.IsNullOrWhiteSpace(query.Text)
            ? []
            : query.Text
                .Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        var normalizedQuery = string.Join(
            ' ',
            terms);

        return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(
            SearchMemories(
                normalizedQuery,
                terms,
                query.Type));
    }

    private AtlasMemoryEntry[] SearchMemories(
        string normalizedQuery,
        string[] terms,
        AtlasMemoryType? type)
    {
        var memories = _memories.Values.AsEnumerable();

        if (type is not null)
        {
            memories = memories.Where(
                memory => memory.Type == type);
        }

        if (terms.Length == 0)
        {
            return
            [
                .. memories
                    .OrderByDescending(memory => memory.CreatedAt)
            ];
        }

        return
        [
            .. memories
                .Where(memory =>
                    terms.All(term =>
                        memory.Content.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase)))
                .Select(memory => new
                {
                    Memory = memory,
                    Score = CalculateRelevance(
                        memory.Content,
                        normalizedQuery,
                        terms)
                })
                .OrderByDescending(result => result.Score)
                .ThenByDescending(result => result.Memory.CreatedAt)
                .Select(result => result.Memory)
        ];
    }

    private static int CalculateRelevance(
        string content,
        string normalizedQuery,
        string[] terms)
    {
        var score = terms.Sum(
            term => CountOccurrences(content, term));

        if (content.Contains(
                normalizedQuery,
                StringComparison.OrdinalIgnoreCase))
        {
            score++;
        }

        return score;
    }

    private static int CountOccurrences(
        string content,
        string term)
    {
        var count = 0;
        var startIndex = 0;

        while (true)
        {
            var index = content.IndexOf(
                term,
                startIndex,
                StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                break;

            count++;
            startIndex = index + term.Length;
        }

        return count;
    }
}