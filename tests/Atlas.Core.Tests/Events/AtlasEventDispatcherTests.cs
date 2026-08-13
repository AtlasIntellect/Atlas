using Atlas.Abstractions.Events;
using Atlas.Core.Events;
using Xunit;

namespace Atlas.Core.Tests.Events;

/// <summary>
/// Represents unit tests for the <see cref="AtlasEventDispatcher"/> class.
/// </summary>
public sealed class AtlasEventDispatcherTests
{
    /// <summary>
    /// Tests that the <see cref="AtlasEventDispatcher.PublishAsync"/> method invokes the registered event handler.
    /// </summary>
    [Fact]
    public async Task PublishAsync_Should_Invoke_Handler()
    {
        var handler = new TestHandler();
        var dispatcher = new AtlasEventDispatcher([handler]);

        await dispatcher.PublishAsync(
            new ApplicationStartedEvent(),
            TestContext.Current.CancellationToken);

        Assert.True(handler.WasCalled);
    }

    /// <summary>
    /// Tests that the <see cref="AtlasEventDispatcher.PublishAsync"/> method does not throw an exception when no handlers are registered for the event type.
    /// </summary>
    [Fact]
    public async Task PublishAsync_Should_Invoke_All_Registered_Handlers()
    {
        var firstHandler = new TestHandler();
        var secondHandler = new TestHandler();

        var dispatcher = new AtlasEventDispatcher(
            [firstHandler, secondHandler]);

        await dispatcher.PublishAsync(
            new ApplicationStartedEvent(),
            TestContext.Current.CancellationToken);

        Assert.True(firstHandler.WasCalled);
        Assert.True(secondHandler.WasCalled);
    }

    /// <summary>
    /// Tests that the <see cref="AtlasEventDispatcher.PublishAsync"/> method does not throw an exception when no handlers are registered for the event type.
    /// </summary>
    [Fact]
    public async Task PublishAsync_Should_Do_Nothing_When_No_Handler_Is_Registered()
    {
        var dispatcher = new AtlasEventDispatcher([]);

        await dispatcher.PublishAsync(
            new ApplicationStartedEvent(),
            TestContext.Current.CancellationToken);

        // Assert that the publish completed without throwing.
        Assert.True(true, "PublishAsync completed without throwing when no handlers are registered.");
    }

    /// <summary>
    /// Represents a test implementation of the <see cref="IAtlasEventHandlerBase"/> interface for testing purposes.
    /// </summary>
    [Fact]
    public async Task PublishAsync_Should_Not_Invoke_Handler_For_Different_Event_Type()
    {
        var handler = new TestHandler();
        var dispatcher = new AtlasEventDispatcher([handler]);

        await dispatcher.PublishAsync(
            new TestEvent(),
            TestContext.Current.CancellationToken);

        Assert.False(handler.WasCalled);
    }

    private sealed record TestEvent : AtlasEvent;

    private sealed class TestHandler : IAtlasEventHandler<ApplicationStartedEvent>
    {
        public bool WasCalled { get; private set; }

        public Task HandleAsync(
            ApplicationStartedEvent @event,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.CompletedTask;
        }
    }
}
