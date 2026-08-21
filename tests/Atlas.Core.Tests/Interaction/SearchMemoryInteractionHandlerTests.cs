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

        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

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

        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

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

        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Same(
            responseFormatter.Response,
            response);
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

        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Same(
            responseFormatter.Response,
            response);
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

        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

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

    /// <summary>
    /// Verifies that the handler formats search results using the response formatter.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_FormatSearchResults()
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

        var queryExtractor = new TestQueryExtractor();

        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

        var response = await handler.HandleAsync(
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            },
            TestContext.Current.CancellationToken);

        Assert.Same(
            responseFormatter.Response,
            response);

        Assert.Equal(
            [memory],
            responseFormatter.ReceivedMemories);
    }

    /// <summary>
    /// Verifies that the handler throws when cancellation has already been requested.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Throw_WhenCancellationRequested()
    {
        var commandDispatcher = new TestCommandDispatcher();
        var queryExtractor = new TestQueryExtractor();
        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that the handler passes the interaction to the query extractor.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassInteraction_ToQueryExtractor()
    {
        var commandDispatcher = new TestCommandDispatcher();
        var queryExtractor = new TestQueryExtractor();
        var responseFormatter = new TestResponseFormatter();

        var handler =
            new SearchMemoryInteractionHandler(
                commandDispatcher,
                queryExtractor,
                responseFormatter);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Same(
            interaction,
            queryExtractor.ReceivedInteraction);
    }

    private sealed class TestQueryExtractor : IAtlasInteractionQueryExtractor
    {
        public string Query { get; init; } = "camera";

        public AtlasInteraction? ReceivedInteraction { get; private set; }

        public string ExtractQuery(AtlasInteraction interaction)
        {
            ReceivedInteraction = interaction;

            return Query;
        }
    }

    private sealed class TestResponseFormatter
        : IAtlasMemorySearchResponseFormatter
    {
        public IReadOnlyList<AtlasMemoryEntry>? ReceivedMemories { get; private set; }

        public AtlasResponse Response { get; init; } = new()
        {
            Content = "Formatted response"
        };

        public AtlasResponse Format(
            IReadOnlyList<AtlasMemoryEntry> memories)
        {
            ReceivedMemories = memories;

            return Response;
        }
    }
}