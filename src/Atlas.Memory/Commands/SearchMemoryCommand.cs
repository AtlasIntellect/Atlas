using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Memory.Commands;

/// <summary>
/// Represents a command to search memories in Atlas.
/// </summary>
/// <param name="Query">The search query.</param>
public sealed record SearchMemoryCommand(
    string Query) : AtlasCommand, IAtlasCommand;