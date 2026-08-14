using Atlas.Abstractions.Commands;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

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
        var dispatcher = new AtlasCommandDispatcher([handler]);

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
        var dispatcher = new AtlasCommandDispatcher([handler]);

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
        var dispatcher = new AtlasCommandDispatcher([handler]);
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
    /// throws when no handler is registered for the command type.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Throw_When_No_Handler_Is_Registered()
    {
        var dispatcher = new AtlasCommandDispatcher([]);

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
    /// throws when the registered handler produces a different result type.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_Throw_When_Result_Type_Does_Not_Match()
    {
        var handler = new TestCommandHandler();
        var dispatcher = new AtlasCommandDispatcher([handler]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync<TestCommand, int>(
                new TestCommand(),
                TestContext.Current.CancellationToken));

        Assert.Contains(
            "does not produce the expected result type",
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
}