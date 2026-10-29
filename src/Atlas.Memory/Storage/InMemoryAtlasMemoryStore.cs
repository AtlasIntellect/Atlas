using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Storage;

/// <summary>
/// Provides an in-memory implementation of <see cref="IAtlasMemoryStore"/>.
/// </summary>
public sealed class InMemoryAtlasMemoryStore : IAtlasMemoryStore
{
    private readonly Dictionary<Guid, AtlasMemoryEntry> _memories = [];

    /// <inheritdoc />
    public Task StoreAsync(
        AtlasMemoryEntry memory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);

        cancellationToken.ThrowIfCancellationRequested();

        _memories[memory.Id] = memory;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(
        AtlasMemoryEntry memory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);

        cancellationToken.ThrowIfCancellationRequested();

        if (!_memories.TryGetValue(memory.Id, out var existingMemory))
            throw new KeyNotFoundException(
                $"No memory exists with identifier '{memory.Id}'.");

        _memories[memory.Id] = new AtlasMemoryEntry
        {
            Id = existingMemory.Id,
            Content = memory.Content,
            CreatedAt = existingMemory.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            Type = memory.Type,
            Interpretation = memory.Interpretation ?? existingMemory.Interpretation,
            LifecycleState = memory.LifecycleState
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<AtlasMemoryEntry?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _memories.TryGetValue(id, out var memory);

        return Task.FromResult(memory);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        AtlasMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        cancellationToken.ThrowIfCancellationRequested();

        var terms =
            string.IsNullOrWhiteSpace(query.Text)
                ? []
                : query.Text
                    .Split(
                        (char[]?)null,
                        StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

        var normalizedQuery =
            string.Join(' ', terms);

        var memories = _memories.Values.AsEnumerable();

        if (!query.IncludeArchived)
        {
            memories = memories.Where(memory =>
                memory.LifecycleState == AtlasMemoryLifecycleState.Active);
        }

        if (query.Type is not null)
        {
            memories = memories.Where(
                memory => memory.Type == query.Type);
        }

        if (terms.Length == 0)
        {
            return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(
            [
                .. memories.OrderByDescending(
                    memory => memory.CreatedAt)
            ]);
        }

        return Task.FromResult<IReadOnlyList<AtlasMemoryEntry>>(
        [
            .. memories
                .Where(
                    memory =>
                        terms.All(
                            term =>
                                memory.Content.Contains(
                                    term,
                                    StringComparison.OrdinalIgnoreCase)))
                .Select(
                    memory => new
                    {
                        Memory = memory,
                        Score = CalculateRelevance(
                            memory.Content,
                            normalizedQuery,
                            terms)
                    })
                .OrderByDescending(
                    result => result.Score)
                .ThenByDescending(
                    result => result.Memory.CreatedAt)
                .Select(
                    result => result.Memory)
        ]);
    }

    /// <inheritdoc />
    public Task ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_memories.TryGetValue(id, out var existingMemory))
            throw new KeyNotFoundException(
                $"No memory exists with identifier '{id}'.");

        // Arhciving an already archived memory is an idempotent no-op.
        if (existingMemory.LifecycleState == AtlasMemoryLifecycleState.Archived)
            return Task.CompletedTask;

        _memories[id] = new AtlasMemoryEntry
        {
            Id = existingMemory.Id,
            Content = existingMemory.Content,
            CreatedAt = existingMemory.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            Type = existingMemory.Type,
            Interpretation = existingMemory.Interpretation,
            LifecycleState = AtlasMemoryLifecycleState.Archived
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Remove returns false when the ID is absent; repeated deletion is harmless.
        _memories.Remove(id);

        return Task.CompletedTask;
    }

    private static int CalculateRelevance(
        string content,
        string normalizedQuery,
        string[] terms)
    {
        var score =
            terms.Sum(
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
            var index =
                content.IndexOf(
                    term,
                    startIndex,
                    StringComparison.OrdinalIgnoreCase);

            if (index < 0)
            {
                break;
            }

            count++;
            startIndex = index + term.Length;
        }

        return count;
    }
}
