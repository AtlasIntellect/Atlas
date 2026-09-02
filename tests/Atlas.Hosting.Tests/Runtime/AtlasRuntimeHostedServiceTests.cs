using Atlas.Hosting.Runtime;
using Atlas.Runtime.Interfaces;
using Xunit;

namespace Atlas.Hosting.Tests.Runtime;

/// <summary>
/// Provides unit tests for the <see cref="AtlasRuntimeHostedService"/> class.
/// </summary>
public sealed class AtlasRuntimeHostedServiceTests
{
    /// <summary>
    /// Verifies that the <see cref="AtlasRuntimeHostedService.StartAsync"/> starts the <see cref="IAtlasRuntime"/>.
    /// </summary>
    [Fact]
    public async Task StartAsync_Should_StartRuntime()
    {
        var runtime = new TestAtlasRuntime();
        var hostedService = new AtlasRuntimeHostedService(runtime);

        await hostedService.StartAsync(
            TestContext.Current.CancellationToken);

        Assert.True(runtime.StartWasCalled);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasRuntimeHostedService.StopAsync"/> stops the <see cref="IAtlasRuntime"/>.
    /// </summary>
    [Fact]
    public async Task StopAsync_Should_StopRuntime()
    {
        var runtime = new TestAtlasRuntime();
        var hostedService = new AtlasRuntimeHostedService(runtime);

        await hostedService.StopAsync(
            TestContext.Current.CancellationToken);

        Assert.True(runtime.StopWasCalled);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasRuntimeHostedService.StartAsync"/> forwards the cancellation token to <see cref="IAtlasRuntime.StartAsync"/>.
    /// </summary>
    [Fact]
    public async Task StartAsync_Should_ForwardCancellationToken()
    {
        var runtime = new TestAtlasRuntime();
        var hostedService = new AtlasRuntimeHostedService(runtime);
        using var cancellationSource = new CancellationTokenSource();

        await hostedService.StartAsync(cancellationSource.Token);

        Assert.Equal(
            cancellationSource.Token,
            runtime.StartCancellationToken);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasRuntimeHostedService.StopAsync"/> forwards the cancellation token to <see cref="IAtlasRuntime.StopAsync"/>.
    /// </summary>
    [Fact]
    public async Task StopAsync_Should_ForwardCancellationToken()
    {
        var runtime = new TestAtlasRuntime();
        var hostedService = new AtlasRuntimeHostedService(runtime);
        using var cancellationSource = new CancellationTokenSource();

        await hostedService.StopAsync(cancellationSource.Token);

        Assert.Equal(
            cancellationSource.Token,
            runtime.StopCancellationToken);
    }

    private sealed class TestAtlasRuntime : IAtlasRuntime
    {
        public bool StartWasCalled { get; private set; }
        public bool StopWasCalled { get; private set; }
        public CancellationToken StartCancellationToken { get; private set; }
        public CancellationToken StopCancellationToken { get; private set; }
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            StartWasCalled = true;
            StartCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            StopWasCalled = true;
            StopCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}