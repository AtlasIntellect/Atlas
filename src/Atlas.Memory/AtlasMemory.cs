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

        var terms = query
            .Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (terms.Length == 0)
        {
            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);
        }

        var results = _memories.Values
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
                    terms)
            })
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Memory.CreatedAt)
            .Select(result => result.Memory)
            .ToList();

        return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(results);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        AtlasMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        cancellationToken.ThrowIfCancellationRequested();

        var memories = _memories.Values.AsEnumerable();

        if (query.Type is not null)
        {
            memories = memories.Where(
                memory => memory.Type == query.Type);
        }

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var terms = query.Text
                .Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (terms.Length == 0)
            {
                return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>([]);
            }

            memories = memories
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
                        terms)
                })
                .OrderByDescending(result => result.Score)
                .ThenByDescending(result => result.Memory.CreatedAt)
                .Select(result => result.Memory);
        }
        else
        {
            memories = memories
                .OrderByDescending(
                    memory => memory.CreatedAt);
        }

        return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(
            [.. memories]);
    }

    private static int CalculateRelevance(
        string content,
        IReadOnlyList<string> terms)
    {
        var score = 0;

        foreach (var term in terms)
        {
            score += CountOccurrences(content, term);
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