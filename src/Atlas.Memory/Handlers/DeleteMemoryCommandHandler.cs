using Atlas.Commands.Interfaces;
using Atlas.Memory.Commands;
using Atlas.Memory.Interfaces;

namespace Atlas.Memory.Handlers;

/// <summary>
/// Handles commands to permanently delete Atlas memories.
/// </summary>
/// <param name="memory">The Atlas memory service.</param>
public class DeleteMemoryCommandHandler(
    IAtlasMemory memory)
    : IAtlasCommandHandler<DeleteMemoryCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> HandleAsync(
        DeleteMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        await memory.DeleteAsync(
            command.MemoryId,
            cancellationToken);

        // True indicates that the deletion operation completed.
        // The memory may already have been absent.
        return true;
    }
}
