using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Commands;

/// <summary>
/// Represents a command to store a memory in Atlas.
/// </summary>
public sealed record StoreMemoryCommand(
    string Content,
    AtlasMemoryType Type,
    IAtlasMemoryData? Data = null) : AtlasCommand, IAtlasCommand;