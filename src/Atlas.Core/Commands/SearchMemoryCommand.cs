using Atlas.Abstractions.Commands;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command to search memories in Atlas.
/// </summary>
/// <param name="Query">The search query.</param>
public sealed record SearchMemoryCommand(
    string Query) : AtlasCommand, IAtlasCommand;