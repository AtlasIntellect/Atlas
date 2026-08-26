using Atlas.Commands.Interfaces;

namespace Atlas.Commands.Models;

/// <summary>
/// Represents a command to retrieve information about the current Atlas instance.
/// </summary>
public sealed record GetAtlasInfoCommand : AtlasCommand, IAtlasCommand;
