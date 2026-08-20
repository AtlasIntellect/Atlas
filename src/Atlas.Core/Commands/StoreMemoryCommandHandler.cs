using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Memory;

namespace Atlas.Core.Commands;

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
            Type = command.Type
        };

        await memory.StoreAsync(
            entry,
            cancellationToken);

        return entry;
    }
}