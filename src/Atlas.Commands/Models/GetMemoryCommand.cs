using Atlas.Commands.Interfaces;

namespace Atlas.Commands.Models;

/// <summary>
/// Represents a command to retrieve a memory from Atlas.
/// </summary>
public sealed record GetMemoryCommand(
    Guid MemoryId) : AtlasCommand, IAtlasCommand;