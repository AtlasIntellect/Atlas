namespace Atlas.Abstractions.AI;

/// <summary>
/// Represents a response returned by an Atlas language model.
/// </summary>
/// <param name="Content">The generated model content.</param>
public sealed record AtlasLanguageModelResponse(
    string Content);