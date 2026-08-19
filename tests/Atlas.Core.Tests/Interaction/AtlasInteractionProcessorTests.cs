using Atlas.Abstractions.Interaction;
using Atlas.Core.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteractionProcessor"/> class.
/// </summary>
public sealed class AtlasInteractionProcessorTests
{
    /// <summary>
    /// Verifies that the processor produces a response for an interaction.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_ReturnResponse()
    {
        var processor = new AtlasInteractionProcessor();

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(
            "Atlas received: Hello Atlas",
            response.Content);
    }

    /// <summary>
    /// Verifies that the processor uses the interaction input when producing
    /// the response.
    /// </summary>
    /// <param name="input">The interaction input.</param>
    /// <param name="expectedContent">The expected response content.</param>
    [Theory]
    [InlineData("Hello Atlas", "Atlas received: Hello Atlas")]
    [InlineData("What camera did I buy?", "Atlas received: What camera did I buy?")]
    [InlineData("Remember this", "Atlas received: Remember this")]
    public async Task ProcessAsync_Should_CreateResponseFromInput(
        string input,
        string expectedContent)
    {
        var processor = new AtlasInteractionProcessor();

        var interaction = new AtlasInteraction
        {
            Input = input
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            expectedContent,
            response.Content);
    }

    /// <summary>
    /// Verifies that the processor throws when cancellation has already been requested.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_Throw_WhenCancellationRequested()
    {
        var processor = new AtlasInteractionProcessor();

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => processor.ProcessAsync(
                interaction,
                cancellationTokenSource.Token));
    }
}