namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Represents the interpretation of an Atlas interaction.
/// </summary>
/// <param name="Intent">The intent identified for the interaction.</param>
/// <param name="Query">The optional query associated with the interaction.</param>
/// <param name="MemoryContent">The optional memory content of the interaction.</param>
public sealed record AtlasInteractionInterpretation(
    AtlasInteractionIntent Intent,
    string? Query,
    string? MemoryContent);