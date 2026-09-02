using Atlas.Abstractions.Configuration;
using Atlas.Events.Interfaces;
using Atlas.Events.Models;
using Atlas.Runtime.Models;
using Microsoft.Extensions.Options;
using Xunit;

namespace Atlas.Runtime.Tests;

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
        var options = Options.Create(new AtlasOptions
        {
            Name = "TestAtlas"
        });

        var applicationContext = new AtlasApplicationContext(options);

        var runtime = new AtlasRuntime(
            dispatcher,
            applicationContext);

        await runtime.StartAsync(
            TestContext.Current.CancellationToken);

        var publishedEvent = Assert.IsType<ApplicationStartedEvent>(
            dispatcher.PublishedEvent);

        Assert.Equal(
            applicationContext.InstanceId,
            publishedEvent.InstanceId);
    }

    /// <summary>
    /// Tests that the <see cref="AtlasRuntime"/> publishes an event when the <see cref="AtlasRuntime.StopAsync(CancellationToken)"/> method is called.
    /// </summary>
    [Fact]
    public async Task StopAsync_Should_Publish_ApplicationStoppingEvent()
    {
        var dispatcher = new TestEventDispatcher();
        var options = Options.Create(new AtlasOptions
        {
            Name = "TestAtlas"
        });

        var applicationContext = new AtlasApplicationContext(options);

        var runtime = new AtlasRuntime(
            dispatcher,
            applicationContext);

        await runtime.StartAsync(
            TestContext.Current.CancellationToken);

        await runtime.StopAsync(
            TestContext.Current.CancellationToken);

        var publishedEvent = Assert.IsType<ApplicationStoppingEvent>(
            dispatcher.PublishedEvent);

        Assert.Equal(
            applicationContext.InstanceId,
            publishedEvent.InstanceId);
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
