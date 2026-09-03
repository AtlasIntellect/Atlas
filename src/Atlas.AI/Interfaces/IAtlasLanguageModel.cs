using Atlas.AI.Models;

namespace Atlas.AI.Interfaces;

/// <summary>
/// Defines a language model that Atlas can use for language generation and understanding.
/// </summary>
public interface IAtlasLanguageModel
{
    /// <summary>
    /// Sends a request to the language model.
    /// </summary>
    /// <param name="request">The language model request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response produced by the language model.</returns>
    Task<AtlasLanguageModelResponse> GenerateAsync(
        AtlasLanguageModelRequest request,
        CancellationToken cancellationToken = default);
}