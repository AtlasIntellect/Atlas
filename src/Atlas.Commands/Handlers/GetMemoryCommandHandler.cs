using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Commands.Handlers;

/// <summary>
/// Handles commands to retrieve a memory from Atlas.
/// </summary>
/// <param name="memory">The Atlas memory service.</param>
public sealed class GetMemoryCommandHandler(
    IAtlasMemory memory)
    : IAtlasCommandHandler<GetMemoryCommand, AtlasMemoryEntry?>
{
    ///<inheritdoc />
    public Task<AtlasMemoryEntry?> HandleAsync(
        GetMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        return memory.GetAsync(
            command.MemoryId,
            cancellationToken);
    }
}