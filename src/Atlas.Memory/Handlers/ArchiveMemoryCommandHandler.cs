using Atlas.Commands.Interfaces;
using Atlas.Memory.Commands;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Handlers;

/// <summary>
/// Handles commands to archive existing Atlas memories.
/// </summary>
/// <param name="memory">The Atlas memory service.</param>
public sealed class ArchiveMemoryCommandHandler(
    IAtlasMemory memory)
    : IAtlasCommandHandler<ArchiveMemoryCommand, AtlasMemoryEntry>
{
    /// <inheritdoc />
    public async Task<AtlasMemoryEntry> HandleAsync(
        ArchiveMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        await memory.ArchiveAsync(
            command.MemoryId,
            cancellationToken);

        return await memory.GetAsync(
            command.MemoryId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"Memory '{command.MemoryId}' was not found after archiving.");
    }
}
