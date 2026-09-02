using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Commands;

/// <summary>
/// Represents a command to process an interaction with Atlas.
/// </summary>
/// <param name="Interaction">The interaction to process.</param>
public sealed record ProcessInteractionCommand(
    AtlasInteraction Interaction) : AtlasCommand, IAtlasCommand;
