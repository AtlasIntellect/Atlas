namespace Atlas.Interaction.Models;

/// <summary>
/// Represents the structured interpretation produced by a language model
/// for an Atlas interaction.
/// </summary>
/// <param name="Intent">The interpreted interaction intent.</param>
/// <param name="Query">The optional memory search query.</param>
/// <param name="MemoryContent">The optional memory content to store.</param>
public sealed record AtlasStructuredInteractionInterpretation(
    AtlasInteractionIntent Intent,
    string? Query,
    string? MemoryContent);
