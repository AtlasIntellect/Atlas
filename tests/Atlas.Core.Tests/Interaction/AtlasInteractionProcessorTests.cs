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

        var queryExtractor = new TestQueryExtractor
        {
            Query = "Hello Atlas"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

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
    [InlineData("How are you?", "Atlas received: How are you?")]
    public async Task ProcessAsync_Should_CreateResponseFromInput(
        string input,
        string expectedContent)
    {
        var commandDispatcher = new TestCommandDispatcher();

        var queryExtractor = new TestQueryExtractor
        {
            Query = "Hello Atlas"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

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

        var queryExtractor = new TestQueryExtractor
        {
            Query = "Hello Atlas"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

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

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "I couldn't find any matching memories.",
            response.Content);
    }

    /// <summary>
    /// Verifies that a memory search interaction dispatches a search command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_DispatchSearchMemoryCommand()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

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
            queryExtractor.Query,
            command.Query);
    }

    /// <summary>
    /// Verifies that the processor passes the cancellation token to the command dispatcher.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_PassCancellationToken_ToCommandDispatcher()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

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

    /// <summary>
    /// Verifies that a memory search interaction includes matching memories
    /// in the produced response.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_ReturnSearchResults()
    {
        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var commandDispatcher = new TestCommandDispatcher
        {
            SearchResults = [memory]
        };

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            memory.Content,
            response.Content);
    }

    /// <summary>
    /// Verifies that a memory storage interaction dispatches a store command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_DispatchStoreMemoryCommand()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D."
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var command =
            Assert.IsType<StoreMemoryCommand>(
                commandDispatcher.ReceivedCommand);

        Assert.Equal(
            interaction.Input,
            command.Content);
    }

    /// <summary>
    /// Verifies that a successful memory storage interaction returns a confirmation.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_ReturnMemoryStoredResponse()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D."
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "Memory stored successfully.",
            response.Content);
    }

    /// <summary>
    /// Verifies that a memory search interaction uses the query extractor
    /// when creating the search command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_UseQueryExtractorForMemorySearch()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var processor =
            new AtlasInteractionProcessor(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera did I buy?"
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var command =
            Assert.IsType<SearchMemoryCommand>(
                commandDispatcher.ReceivedCommand);

        Assert.Equal(
            "camera",
            command.Query);

        Assert.Same(
            interaction,
            queryExtractor.ReceivedInteraction);
    }

    private sealed class TestCommandDispatcher : IAtlasCommandDispatcher
    {
        public IAtlasCommand? ReceivedCommand { get; private set; }

        public IReadOnlyList<AtlasMemoryEntry> SearchResults { get; init; } = [];

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<TResult> DispatchAsync<TCommand, TResult>(
            TCommand command,
            CancellationToken cancellationToken = default)
            where TCommand : IAtlasCommand
        {
            ReceivedCommand = command;
            ReceivedCancellationToken = cancellationToken;

            if (command is SearchMemoryCommand)
            {
                return Task.FromResult(
                    (TResult)SearchResults);
            }

            if (command is StoreMemoryCommand storeCommand)
            {
                return Task.FromResult(
                    (TResult)(object)new AtlasMemoryEntry
                    {
                        Id = Guid.NewGuid(),
                        Content = storeCommand.Content,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
            }

            throw new InvalidOperationException(
                $"Unexpected command type: {typeof(TCommand).FullName}");
        }
    }

    private sealed class TestQueryExtractor
        : IAtlasInteractionQueryExtractor
    {
        public string Query { get; init; } = string.Empty;

        public AtlasInteraction? ReceivedInteraction { get; private set; }

        public string ExtractQuery(
            AtlasInteraction interaction)
        {
            ReceivedInteraction = interaction;

            return Query;
        }
    }
}