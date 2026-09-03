using Atlas.AI.Interfaces;
using Atlas.AI.Models;

namespace Atlas.AI.Tests.TestImplementations;

/// <summary>
/// Provides a deterministic language model implementation for testing.
/// </summary>
public sealed class TestAtlasLanguageModel
    : IAtlasLanguageModel
{
    /// <summary>
    /// Gets the request received by the language model.
    /// </summary>
    public AtlasLanguageModelRequest? ReceivedRequest { get; private set; }

    /// <summary>
    /// Gets the cancellation token received by the language model.
    /// </summary>
    public CancellationToken ReceivedCancellationToken { get; private set; }

    /// <summary>
    /// Gets or sets the response to return from the language model.
    /// </summary>
    public AtlasLanguageModelResponse Response { get; init; } = new(
        "Test response");

    /// <summary>
    /// Generates a response based on the provided request.
    /// </summary>
    /// <param name="request">The language model request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated language model response.</returns>
    public Task<AtlasLanguageModelResponse> GenerateAsync(
        AtlasLanguageModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        ReceivedRequest = request;
        ReceivedCancellationToken = cancellationToken;

        return Task.FromResult(Response);
    }
}
