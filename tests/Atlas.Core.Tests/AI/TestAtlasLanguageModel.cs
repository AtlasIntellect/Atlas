using Atlas.Abstractions.AI;

namespace Atlas.Core.Tests.AI;

/// <summary>
/// Provides a deterministic language model implementation for testing.
/// </summary>
internal sealed class TestAtlasLanguageModel
    : IAtlasLanguageModel
{
    public AtlasLanguageModelRequest? ReceivedRequest { get; private set; }

    public CancellationToken ReceivedCancellationToken { get; private set; }

    public AtlasLanguageModelResponse Response { get; init; } =
        new("Test response.");

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