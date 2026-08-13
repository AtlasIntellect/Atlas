using Atlas.Abstractions.Events;
using Atlas.Abstractions.Runtime;
using Atlas.Core.Events;
using Atlas.Core.Runtime;
using Atlas.Hosting.DependencyInjection;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Atlas.Hosting.Tests.DependencyInjection;

/// <summary>
/// Provides unit tests for the <see cref="AtlasServiceCollectionExtensions"/> class.
/// </summary>
public sealed class AtlasServiceCollectionExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="AtlasServiceCollectionExtensions.AddAtlas"/> registers the core Atlas services.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterCoreServices()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        using var provider = services.BuildServiceProvider();

        Assert.IsType<AtlasEventDispatcher>(
            provider.GetRequiredService<IAtlasEventDispatcher>());

        Assert.IsType<AtlasRuntime>(
            provider.GetRequiredService<IAtlasRuntime>());

        Assert.IsType<StartupHandler>(
            provider.GetRequiredService<IAtlasEventHandlerBase>());

        Assert.Contains(
            provider.GetServices<IHostedService>(),
            service => service is AtlasRuntimeHostedService);
    }
}
