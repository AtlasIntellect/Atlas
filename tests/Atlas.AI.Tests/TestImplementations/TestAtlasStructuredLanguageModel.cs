using Atlas.AI.Interfaces;
using Atlas.AI.Structured;

namespace Atlas.AI.Tests.TestImplementations;

/// <summary>
/// Provides a deterministic language model implementation for testing.
/// </summary>
public sealed class TestAtlasStructuredLanguageModel
    : IAtlasStructuredLanguageModel
{
    /// <summary>
    /// Gets the request received by the language model.
    /// </summary>
    public AtlasStructuredLanguageModelRequest? ReceivedRequest { get; private set; }

    /// <summary>
    /// Gets the cancellation token received by the language model.
    /// </summary>
    public CancellationToken ReceivedCancellationToken { get; private set; }

    /// <summary>
    /// Gets or sets the response returned by the language model.
    /// </summary>
    public AtlasStructuredLanguageModelResponse Response { get; init; } =
        new("Test structured response");

    /// <inheritdoc/>
    public Task<AtlasStructuredLanguageModelResponse> GenerateAsync(
        AtlasStructuredLanguageModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        ReceivedRequest = request;
        ReceivedCancellationToken = cancellationToken;

        return Task.FromResult(Response);
    }
}
