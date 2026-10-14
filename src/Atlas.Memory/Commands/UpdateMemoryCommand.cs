using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Commands;

/// <summary>
/// Represents a command to update an existing memory in Atlas.
/// </summary>
/// <param name="MemoryId">The identifier of the memory to update.</param>
/// <param name="Content">The updated memory content.</param>
/// <param name="Type">The updated memory type.</param>
/// <param name="Data">
/// Optional replacement interpretation data. When null, the existing
/// interpretation is preserved.
/// </param>
public sealed record UpdateMemoryCommand(
    Guid MemoryId,
    string Content,
    AtlasMemoryType Type,
    IAtlasMemoryData? Data = null) : AtlasCommand, IAtlasCommand;
