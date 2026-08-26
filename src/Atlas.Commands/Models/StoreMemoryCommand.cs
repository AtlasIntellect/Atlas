using Atlas.Commands.Interfaces;

namespace Atlas.Commands.Models;

/// <summary>
/// Represents a command to store a memory in Atlas.
/// </summary>
public sealed record StoreMemoryCommand(
    string Content,
    AtlasMemoryType Type,
    IAtlasMemoryData? Data = null) : AtlasCommand, IAtlasCommand;