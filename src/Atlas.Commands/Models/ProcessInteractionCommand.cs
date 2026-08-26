using Atlas.Commands.Interfaces;

namespace Atlas.Commands.Models;

/// <summary>
/// Represents a command to process an interaction with Atlas.
/// </summary>
/// <param name="Interaction">The interaction to process.</param>
public sealed record ProcessInteractionCommand(
    AtlasInteraction Interaction) : AtlasCommand, IAtlasCommand;
