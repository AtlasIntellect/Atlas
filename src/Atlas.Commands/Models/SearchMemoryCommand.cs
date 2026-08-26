using Atlas.Commands.Interfaces;

namespace Atlas.Commands.Models;

/// <summary>
/// Represents a command to search memories in Atlas.
/// </summary>
/// <param name="Query">The search query.</param>
public sealed record SearchMemoryCommand(
    string Query) : AtlasCommand, IAtlasCommand;