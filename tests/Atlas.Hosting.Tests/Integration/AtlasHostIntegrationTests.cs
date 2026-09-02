using Atlas.Hosting.DependencyInjection;
using Atlas.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Atlas.Hosting.Tests.Integration;

/// <summary>
/// Provides integration tests for the Atlas Generic Host integration.
/// </summary>
public sealed class AtlasHostIntegrationTests
{
    /// <summary>
    /// Verifies that starting the Generic Host starts the <see cref="IAtlasRuntime"/>.
    /// </summary>
    [Fact]
    public async Task StartAsync_Should_StartAtlasRuntime()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddAtlas(builder.Configuration);

        var runtime = new TestAtlasRuntime();

        builder.Services.AddSingleton<IAtlasRuntime>(runtime);

        using var host = builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        Assert.True(runtime.StartWasCalled);

        await host.StopAsync(
            TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that stopping the Generic Host stops the <see cref="IAtlasRuntime"/>.
    /// </summary>
    [Fact]
    public async Task StopAsync_Should_StopAtlasRuntime()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddAtlas(builder.Configuration);

        var runtime = new TestAtlasRuntime();

        builder.Services.AddSingleton<IAtlasRuntime>(runtime);

        using var host = builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        await host.StopAsync(
            TestContext.Current.CancellationToken);

        Assert.True(runtime.StopWasCalled);
    }

    private sealed class TestAtlasRuntime : IAtlasRuntime
    {
        public bool StartWasCalled { get; private set; }

        public bool StopWasCalled { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            StartWasCalled = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            StopWasCalled = true;

            return Task.CompletedTask;
        }
    }
}