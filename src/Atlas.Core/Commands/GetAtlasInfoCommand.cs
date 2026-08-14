using Atlas.Abstractions.Commands;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command to retrieve information about the current Atlas instance.
/// </summary>
public sealed record GetAtlasInfoCommand : AtlasCommand, IAtlasCommand;
