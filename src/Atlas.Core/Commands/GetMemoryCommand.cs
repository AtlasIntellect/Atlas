using Atlas.Abstractions.Commands;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command to retrieve a memory from Atlas.
/// </summary>
public sealed record GetMemoryCommand(
    Guid MemoryId) : AtlasCommand, IAtlasCommand;