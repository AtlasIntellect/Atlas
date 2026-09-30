using Atlas.Memory.Interfaces;
using Atlas.Memory.Mapping;
using Atlas.Memory.Models;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Memory.Storage;

/// <summary>
/// Provides an Entity Framework Core implementation of <see cref="IAtlasMemoryStore"/>
/// </summary>
/// <param name="dbContext"></param>
public sealed class EntityFrameworkAtlasMemoryStore(
    AtlasMemoryDbContext dbContext)
    : IAtlasMemoryStore
{
    /// <inheritdoc />
    public async Task StoreAsync(
        AtlasMemoryEntry memory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memory);

        cancellationToken.ThrowIfCancellationRequested();

        var record = AtlasMemoryRecordMapper.ToRecord(memory);

        var existing =
            await dbContext.Memories
                .SingleOrDefaultAsync(
                    candidate => candidate.Id == record.Id,
                    cancellationToken);

        if (existing is null)
            await dbContext.Memories.AddAsync(record, cancellationToken);
        else
            dbContext.Entry(existing).CurrentValues.SetValues(record);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AtlasMemoryEntry?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record =
            await dbContext.Memories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                memory => memory.Id == id,
                cancellationToken);

        return record is null ?
            null : AtlasMemoryRecordMapper.ToMemory(record);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
        AtlasMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        cancellationToken.ThrowIfCancellationRequested();

        var records =
            await dbContext.Memories
                .AsNoTracking()
                .Where(memory =>
                    query.Type == null || memory.Type == query.Type)
                .ToListAsync(cancellationToken);

        var memories =
            records.Select(AtlasMemoryRecordMapper.ToMemory).AsEnumerable();

        var terms =
            string.IsNullOrWhiteSpace(query.Text) ? [] : query.Text.Split(
                (char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (terms.Length == 0)
        {
            return
            [
                .. memories.OrderByDescending(memory => memory.CreatedAt)
            ];
        }

        var normalizedQuery = string.Join(' ', terms);

        return
        [
            .. memories
                .Where(memory => 
                    terms.All(term => 
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
        ];
    }

    private static int CalculateRelevance(
        string content,
        string normalizedQuery,
        string[] terms)
    {
        var score =
            terms.Sum(term =>
                    CountOccurrences(content, term));

        if (content.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            score++;

        return score;
    }

    private static int CountOccurrences(string content, string term)
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
