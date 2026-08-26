namespace Atlas.Abstractions.AI;

/// <summary>
/// Represents a request sent to an Atlas language model.
/// </summary>
/// <param name="Prompt">The prompt sent to the language model.</param>
/// <param name="ResponseFormat">The expected structured response format.</param>
public sealed record AtlasLanguageModelRequest(
    string Prompt,
    string? ResponseFormat = null);