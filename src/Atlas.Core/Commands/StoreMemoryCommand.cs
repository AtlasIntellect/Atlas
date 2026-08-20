using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Memory;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command to store a memory in Atlas.
/// </summary>
public sealed record StoreMemoryCommand(
    string Content,
    AtlasMemoryType Type) : AtlasCommand, IAtlasCommand;