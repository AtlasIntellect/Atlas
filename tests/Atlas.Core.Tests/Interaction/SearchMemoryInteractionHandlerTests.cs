using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;
using Atlas.Core.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="SearchMemoryInteractionHandler"/> class.
/// </summary>
public sealed class SearchMemoryInteractionHandlerTests
{
    /// <summary>
    /// Verifies that the handler reports the correct intent.
    /// </summary>
    [Fact]
    public void Intent_Should_ReturnSearchMemory()
    {
        var commandDispatcher = new TestCommandDispatcher();
        var queryExtractor = new TestQueryExtractor();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor);

        Assert.Equal(
            AtlasInteractionIntent.SearchMemory,
            handler.Intent);
    }

    /// <summary>
    /// Verifies that the handler extracts the query and dispatches a search command.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_DispatchSearchMemoryCommand()
    {
        var commandDispatcher = new TestCommandDispatcher();
        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var command =
            Assert.IsType<SearchMemoryCommand>(
                commandDispatcher.ReceivedCommand);

        Assert.Equal(
            "camera",
            command.Query);
    }

    /// <summary>
    /// Verifies that the handler returns matching memory content.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnMatchingMemoryContent()
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

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            memory.Content,
            response.Content);
    }

    /// <summary>
    /// Verifies that the handler returns the no-results response when no memories match.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnNoResultsResponse_WhenNoMemoriesMatch()
    {
        var commandDispatcher = new TestCommandDispatcher();
        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "I couldn't find any matching memories.",
            response.Content);
    }

    /// <summary>
    /// Verifies that the handler passes the cancellation token to the command dispatcher.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassCancellationToken_ToCommandDispatcher()
    {
        var commandDispatcher = new TestCommandDispatcher();
        var queryExtractor = new TestQueryExtractor
        {
            Query = "camera"
        };

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor);

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await handler.HandleAsync(
            interaction,
            cancellationToken);

        Assert.Equal(
            cancellationToken,
            commandDispatcher.ReceivedCancellationToken);
    }

    /// <summary>
    /// Provides a test implementation of the command dispatcher.
    /// </summary>
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

            throw new InvalidOperationException(
                $"Unexpected command type: {typeof(TCommand).FullName}");
        }
    }

    private sealed class TestQueryExtractor : IAtlasInteractionQueryExtractor
    {
        public string Query { get; init; } = "camera";

        public string ExtractQuery(AtlasInteraction interaction)
        {
            return Query;
        }
    }
}