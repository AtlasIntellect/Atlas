using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command to process an interaction with Atlas.
/// </summary>
/// <param name="Interaction">The interaction to process.</param>
public sealed record ProcessInteractionCommand(
    AtlasInteraction Interaction) : AtlasCommand, IAtlasCommand;
