using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Memory.Commands;

/// <summary>
/// Represents a command to retrieve a memory from Atlas.
/// </summary>
public sealed record GetMemoryCommand(
    Guid MemoryId) : AtlasCommand, IAtlasCommand;