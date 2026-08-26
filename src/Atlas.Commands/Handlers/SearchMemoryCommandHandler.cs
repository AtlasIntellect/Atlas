using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Commands.Handlers;

/// <summary>
/// Handles commands to search memories in Atlas.
/// </summary>
/// <param name="memory">The Atlas memory service.</param>
public sealed class SearchMemoryCommandHandler(
    IAtlasMemory memory)
    : IAtlasCommandHandler<
        SearchMemoryCommand,
        IReadOnlyList<AtlasMemoryEntry>>
{
    /// <inheritdoc />
    public Task<IReadOnlyList<AtlasMemoryEntry>> HandleAsync(
        SearchMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        return memory.SearchAsync(
            command.Query,
            cancellationToken);
    }
}