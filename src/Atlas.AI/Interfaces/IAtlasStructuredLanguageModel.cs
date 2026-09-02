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
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    /// <param name="request">The structured language model request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated structured response.</returns>
    Task<TResponse> GenerateAsync<TResponse>(
        AtlasStructuredLanguageModelRequest request,
        CancellationToken cancellationToken = default);
}