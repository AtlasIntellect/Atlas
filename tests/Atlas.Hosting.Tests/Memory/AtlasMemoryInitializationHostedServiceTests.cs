using Atlas.Hosting.Memory;
using Atlas.Memory.Interfaces;
using Xunit;

namespace Atlas.Hosting.Tests.Memory;

/// <summary>
/// Provides tests for the <see cref="AtlasMemoryInitializationHostedService"/> class.
/// </summary>
public sealed class AtlasMemoryInitializationHostedServiceTests
{
    /// <summary>
    /// Verifies that starting the hosted service initializes the memory store.
    /// </summary>
    [Fact]
    public async Task StartAsync_Should_InitializeMemoryStore()
    {
        var initializer =
            new TestMemoryStoreInitializer();

        var hostedService =
            new AtlasMemoryInitializationHostedService(
                initializer);

        var cancellationToken =
            TestContext.Current.CancellationToken;

        await hostedService.StartAsync(cancellationToken);

        Assert.True(initializer.WasInitialized);
        Assert.Equal(
            cancellationToken,
            initializer.ReceivedCancellationToken);
    }

    private sealed class TestMemoryStoreInitializer
        : IAtlasMemoryStoreInitializer
    {
        public bool WasInitialized { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task InitializeAsync(
            CancellationToken cancellationToken = default)
        {
            WasInitialized = true;
            ReceivedCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}
