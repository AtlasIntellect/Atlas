using Atlas.Interaction.Handlers;
using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Handlers;

/// <summary>
/// Provides unit tests for the <see cref="UnknownInteractionHandler"/> class.
/// </summary>
public sealed class UnknownInteractionHandlerTests
{
    /// <summary>
    /// Verifies that the handler represents the unknown intent.
    /// </summary>
    [Fact]
    public void Intent_Should_ReturnUnknown()
    {
        var handler = new UnknownInteractionHandler();

        Assert.Equal(
            AtlasInteractionIntent.Unknown,
            handler.Intent);
    }

    /// <summary>
    /// Verifies that the handler returns the existing fallback response.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnFallbackResponse()
    {
        var handler = new UnknownInteractionHandler();

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.Unknown,
                null,
                null);

        var response = await handler.HandleAsync(
            interaction,
            interpretation,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "Atlas received: Hello Atlas",
            response.Content);
    }

    /// <summary>
    /// Verifies that the handler respects cancellation.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Throw_WhenCancellationRequested()
    {
        var handler = new UnknownInteractionHandler();

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var interpretation =
            new AtlasInteractionInterpretation(
                AtlasInteractionIntent.Unknown,
                null,
                null);

        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                interaction,
                interpretation,
                cancellationTokenSource.Token));
    }
}