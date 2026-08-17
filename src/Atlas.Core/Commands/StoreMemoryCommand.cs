using Atlas.Abstractions.Commands;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command to store a memory in Atlas.
/// </summary>
public sealed record StoreMemoryCommand(
    string Content) : AtlasCommand, IAtlasCommand;