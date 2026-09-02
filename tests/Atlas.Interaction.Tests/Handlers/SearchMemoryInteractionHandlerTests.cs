using Atlas.Commands.Interfaces;
using Atlas.Interaction.Handlers;
using Atlas.Interaction.Models;
using Atlas.Memory.Commands;
using Atlas.Memory.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Handlers;

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

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

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

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await handler.HandleAsync(
            interaction,
            CreateSearchInterpretation("camera"),
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

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await handler.HandleAsync(
            interaction,
            CreateSearchInterpretation(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
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

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        var response = await handler.HandleAsync(
            interaction,
            CreateSearchInterpretation(),
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

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var interaction = new AtlasInteraction
        {
            Input = "What camera do I have?"
        };

        await handler.HandleAsync(
            interaction,
            CreateSearchInterpretation(),
            cancellationToken);

        Assert.Equal(
            cancellationToken,
            commandDispatcher.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that the handler formats multiple search results into the response.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnFormattedMemoryContent()
    {
        var firstMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var secondMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "My camera has an 18-55mm lens.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var commandDispatcher = new TestCommandDispatcher
        {
            SearchResults = [firstMemory, secondMemory]
        };

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        var response = await handler.HandleAsync(
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            },
            CreateSearchInterpretation(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            $"{firstMemory.Content}{Environment.NewLine}{secondMemory.Content}",
            response.Content);
    }

    /// <summary>
    /// Verifies that the handler throws when cancellation has already been requested.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Throw_WhenCancellationRequested()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                CreateSearchInterpretation(),
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that the
    /// <see cref="SearchMemoryInteractionHandler.HandleAsync"/> method
    /// throws an <see cref="InvalidOperationException"/> when the provided
    /// <see cref="AtlasInteractionInterpretation"/> does not contain a query.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Throw_WhenInterpretationHasNoQuery()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var handler =
            new SearchMemoryInteractionHandler(commandDispatcher);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                new AtlasInteraction
                {
                    Input = "What camera do I have?"
                },
                new AtlasInteractionInterpretation(
                    AtlasInteractionIntent.SearchMemory,
                    null,
                    null),
                TestContext.Current.CancellationToken));
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

            throw new InvalidOperationException(
                $"Unexpected command type: {typeof(TCommand).FullName}");
        }
    }

    private static AtlasInteractionInterpretation CreateSearchInterpretation(
        string query = "camera")
    {
        return new AtlasInteractionInterpretation(
            AtlasInteractionIntent.SearchMemory,
            query,
            null);
    }
}