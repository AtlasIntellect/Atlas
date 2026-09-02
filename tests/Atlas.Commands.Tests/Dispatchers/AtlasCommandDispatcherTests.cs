using Atlas.Commands.Dispatchers;
using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;
using Xunit;

namespace Atlas.Commands.Tests.Dispatchers;

/// <summary>
/// Provides unit tests for the <see cref="AtlasCommandDispatcher"/> class.
/// </summary>
public sealed class AtlasCommandDispatcherTests
{
    /// <summary>
    /// Verifies that <see cref="AtlasCommandDispatcher.DispatchAsync{TCommand, TResult}"/>
    /// invokes the registered command handler.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Invoke_Handler()
    {
        var handler = new TestCommandHandler();


        var dispatcher =
            new AtlasCommandDispatcher(
                new TestServiceProvider(handler));

        await dispatcher.DispatchAsync<TestCommand, string>(
            new TestCommand(),
            TestContext.Current.CancellationToken);

        Assert.True(handler.WasCalled);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasCommandDispatcher.DispatchAsync{TCommand, TResult}"/>
    /// returns the result produced by the command handler.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Return_Handler_Result()
    {
        var handler = new TestCommandHandler();

        var dispatcher =
            new AtlasCommandDispatcher(
                new TestServiceProvider(handler));

        var result = await dispatcher.DispatchAsync<TestCommand, string>(
            new TestCommand(),
            TestContext.Current.CancellationToken);

        Assert.Equal("Test result", result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasCommandDispatcher.DispatchAsync{TCommand, TResult}"/>
    /// passes the cancellation token to the command handler.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Pass_CancellationToken_To_Handler()
    {
        var handler = new TestCommandHandler();

        var dispatcher =
            new AtlasCommandDispatcher(
                new TestServiceProvider(handler));
        
        using var cancellationTokenSource = new CancellationTokenSource();

        await dispatcher.DispatchAsync<TestCommand, string>(
            new TestCommand(),
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            handler.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasCommandDispatcher.DispatchAsync{TCommand, TResult}"/>
    /// throws when the requested command handler is not registered with dependency injection.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Throw_When_Handler_Is_Not_Registered()
    {
        var dispatcher =
            new AtlasCommandDispatcher(
                new TestServiceProvider(null));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync<TestCommand, string>(
                new TestCommand(),
                TestContext.Current.CancellationToken));

        Assert.Contains(
            "No handler registered for command type",
            exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasCommandDispatcher.DispatchAsync{TCommand, TResult}"/>
    /// throws when the requested result type is not registered.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Throw_When_HandlerForRequestedResultType_Is_NotRegistered()
    {
        var handler = new TestCommandHandler();

        var dispatcher =
            new AtlasCommandDispatcher(
                new TestServiceProvider(handler));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync<TestCommand, int>(
                new TestCommand(),
                TestContext.Current.CancellationToken));

        Assert.Contains(
            "No handler registered for command type",
            exception.Message);
    }

    private sealed record TestCommand : AtlasCommand;

    private sealed class TestCommandHandler
        : IAtlasCommandHandler<TestCommand, string>
    {
        public bool WasCalled { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<string> HandleAsync(
            TestCommand command,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult("Test result");
        }
    }

    private sealed class TestServiceProvider(
        IAtlasCommandHandler<TestCommand, string>? handler)
        : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType ==
                typeof(IAtlasCommandHandler<TestCommand, string>))
            {
                return handler;
            }

            return null;
        }
    }
}