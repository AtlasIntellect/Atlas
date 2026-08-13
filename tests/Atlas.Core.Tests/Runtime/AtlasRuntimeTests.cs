using Atlas.Abstractions.Events;
using Atlas.Core.Runtime;
using Xunit;

namespace Atlas.Core.Tests.Runtime;

/// <summary>
/// Tests for the <see cref="AtlasRuntime"/> class.
/// </summary>
public sealed class AtlasRuntimeTests
{
    /// <summary>
    /// Tests that the <see cref="AtlasRuntime"/> publishes an event when the <see cref="AtlasRuntime.StartAsync(CancellationToken)"/> method is called.
    /// </summary>
    [Fact]
    public async Task StartAsync_Should_Publish_ApplicationStartedEvent()
    {
        var dispatcher = new TestEventDispatcher();
        var runtime = new AtlasRuntime(dispatcher);

        await runtime.StartAsync(
            TestContext.Current.CancellationToken);

        Assert.IsType<ApplicationStartedEvent>(
            dispatcher.PublishedEvent);
    }

    private sealed class TestEventDispatcher : IAtlasEventDispatcher
    {
        public AtlasEvent? PublishedEvent { get; private set; }

        public Task PublishAsync(AtlasEvent @event, CancellationToken cancellationToken = default)
        {
            PublishedEvent = @event;

            return Task.CompletedTask;
        }
    }
}
