using Atlas.AI.Structured;

namespace Atlas.AI.Interfaces;

/// <summary>
/// Defines a language model capable of producing structured responses.
/// </summary>
public interface IAtlasStructuredLanguageModel
{
    /// <summary>
    /// Generates a structured response.
    /// </summary>
    /// <param name="request">The structured language model request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated structured response.</returns>
    Task<AtlasStructuredLanguageModelResponse> GenerateAsync(
        AtlasStructuredLanguageModelRequest request,
        CancellationToken cancellationToken = default);
}