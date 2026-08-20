using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;
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
        var commandDispatcher = new TestCommandDispatcher();

        var processor =
            new AtlasInteractionProcessor(commandDispatcher);

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
    [InlineData("Remember this", "Atlas received: Remember this")]
    [InlineData("How are you?", "Atlas received: How are you?")]
    public async Task ProcessAsync_Should_CreateResponseFromInput(
        string input,
        string expectedContent)
    {
        var commandDispatcher = new TestCommandDispatcher();

        var processor =
            new AtlasInteractionProcessor(commandDispatcher);

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
        var commandDispatcher = new TestCommandDispatcher();

        var processor =
            new AtlasInteractionProcessor(commandDispatcher);

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

    /// <summary>
    /// Verifies that the processor recognizes a memory search interaction.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_RecognizeMemorySearch()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var processor =
            new AtlasInteractionProcessor(commandDispatcher);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "Atlas detected a memory search.",
            response.Content);
    }

    /// <summary>
    /// Verifies that a memory search interaction dispatches a search command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_DispatchSearchMemoryCommand()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var processor =
            new AtlasInteractionProcessor(commandDispatcher);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var command =
            Assert.IsType<SearchMemoryCommand>(
                commandDispatcher.ReceivedCommand);

        Assert.Equal(
            interaction.Input,
            command.Query);
    }

    /// <summary>
    /// Verifies that the processor passes the cancellation token to the command dispatcher.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_PassCancellationToken_ToCommandDispatcher()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var processor =
            new AtlasInteractionProcessor(commandDispatcher);

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await processor.ProcessAsync(
            interaction,
            cancellationToken);

        Assert.Equal(
            cancellationToken,
            commandDispatcher.ReceivedCancellationToken);
    }

    private sealed class TestCommandDispatcher : IAtlasCommandDispatcher
    {
        public IAtlasCommand? ReceivedCommand { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<TResult> DispatchAsync<TCommand, TResult>(
            TCommand command,
            CancellationToken cancellationToken = default)
            where TCommand : IAtlasCommand
        {
            ReceivedCommand = command;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(
                (TResult)(object)Array.Empty<AtlasMemoryEntry>());
        }
    }
}