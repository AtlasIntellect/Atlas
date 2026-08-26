namespace Atlas.Abstractions.AI;

/// <summary>
/// Represents a request for structured output from a language model.
/// </summary>
/// <param name="Prompt">The prompt sent to the language model.</param>
/// <param name="ResponseType">The expected structured response type.</param>
public sealed record AtlasStructuredLanguageModelRequest(
    string Prompt,
    Type ResponseType);