using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;
using Atlas.Core.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="StoreMemoryInteractionHandler"/> class.
/// </summary>
public sealed class StoreMemoryInteractionHandlerTests
{
    /// <summary>
    /// Verifies that the handler reports the correct intent.
    /// </summary>
    [Fact]
    public void Intent_Should_ReturnStoreMemory()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var contentExtractor = new TestContentExtractor
        {
            Content = "I bought a Canon EOS 350D camera."
        };

        var typeClassifier = new TestMemoryTypeClassifier
        {
            Type = AtlasMemoryType.Fact
        };

        var handler =
            new StoreMemoryInteractionHandler(
                commandDispatcher,
                contentExtractor,
                typeClassifier);

        Assert.Equal(
            AtlasInteractionIntent.StoreMemory,
            handler.Intent);
    }

    /// <summary>
    /// Verifies that the handler dispatches a store memory command.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_DispatchStoreMemoryCommand()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var contentExtractor = new TestContentExtractor
        {
            Content = "I bought a Canon EOS 350D camera."
        };

        var typeClassifier = new TestMemoryTypeClassifier
        {
            Type = AtlasMemoryType.Fact
        };

        var handler =
            new StoreMemoryInteractionHandler(
                commandDispatcher,
                contentExtractor,
                typeClassifier);

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D."
        };

        await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var command =
            Assert.IsType<StoreMemoryCommand>(
                commandDispatcher.ReceivedCommand);

        Assert.Equal(
            contentExtractor.Content,
            command.Content);
    }

    /// <summary>
    /// Verifies that the handler returns a successful storage response.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnMemoryStoredResponse()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var contentExtractor = new TestContentExtractor
        {
            Content = "I bought a Canon EOS 350D camera."
        };

        var typeClassifier = new TestMemoryTypeClassifier
        {
            Type = AtlasMemoryType.Fact
        };

        var handler =
            new StoreMemoryInteractionHandler(
                commandDispatcher,
                contentExtractor,
                typeClassifier);

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D."
        };

        var response = await handler.HandleAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "Memory stored successfully.",
            response.Content);
    }

    /// <summary>
    /// Verifies that the handler passes the cancellation token to the command dispatcher.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_PassCancellationToken_ToCommandDispatcher()
    {
        var commandDispatcher = new TestCommandDispatcher();

        var contentExtractor = new TestContentExtractor
        {
            Content = "I bought a Canon EOS 350D camera."
        };

        var typeClassifier = new TestMemoryTypeClassifier
        {
            Type = AtlasMemoryType.Fact
        };

        var handler =
            new StoreMemoryInteractionHandler(
                commandDispatcher,
                contentExtractor,
                typeClassifier);

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D."
        };

        await handler.HandleAsync(
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

    private sealed class TestContentExtractor
        : IAtlasInteractionMemoryContentExtractor
    {
        public string Content { get; init; } = string.Empty;

        public string ExtractContent(
            AtlasInteraction interaction)
        {
            return Content;
        }
    }

    private sealed class TestMemoryTypeClassifier
        : IAtlasMemoryTypeClassifier
    {
        public AtlasMemoryType Type { get; init; }

        public AtlasMemoryType Classify(string content)
        {
            return Type;
        }
    }
}