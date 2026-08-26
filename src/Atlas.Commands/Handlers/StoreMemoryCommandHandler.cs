using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Commands.Handlers;

/// <summary>
/// 
/// </summary>
/// <param name="memory">The Atlas memory service.</param>
public sealed class StoreMemoryCommandHandler(
    IAtlasMemory memory)
    : IAtlasCommandHandler<StoreMemoryCommand, AtlasMemoryEntry>
{
    /// <inheritdoc/>
    public async Task<AtlasMemoryEntry> HandleAsync(
        StoreMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = command.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            Type = command.Type,
            Interpretation = command.Data is null
                ? null
                : new AtlasMemoryInterpretation
                {
                    Data = command.Data
                }
        };

        await memory.StoreAsync(
            entry,
            cancellationToken);

        return entry;
    }
}