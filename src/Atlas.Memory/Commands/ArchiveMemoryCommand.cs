using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Memory.Commands;

/// <summary>
/// Represents a command to archive an existing memory in Atlas.
/// </summary>
/// <param name="MemoryId">The identified of the memory to archive.</param>
public sealed record ArchiveMemoryCommand(
    Guid MemoryId) : AtlasCommand, IAtlasCommand;
