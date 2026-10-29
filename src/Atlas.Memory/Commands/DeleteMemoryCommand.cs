using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Memory.Commands;

/// <summary>
/// Represents a command to permanently delete a memory from Atlas.
/// </summary>
/// <param name="MemoryId">The identifier of the memory to delete.</param>
public sealed record DeleteMemoryCommand(
    Guid MemoryId) : AtlasCommand, IAtlasCommand;
