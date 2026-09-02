using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;

namespace Atlas.Runtime.Commands;

/// <summary>
/// Represents a command to retrieve information about the current Atlas instance.
/// </summary>
public sealed record GetAtlasInfoCommand : AtlasCommand, IAtlasCommand;
