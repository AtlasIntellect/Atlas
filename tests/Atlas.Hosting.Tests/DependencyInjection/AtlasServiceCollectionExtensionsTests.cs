using Atlas.Abstractions.Configuration;
using Atlas.Abstractions.Events;
using Atlas.Abstractions.Runtime;
using Atlas.Core.Events;
using Atlas.Core.Runtime;
using Atlas.Hosting.DependencyInjection;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

    /// <summary>
    /// Verifies that <see cref="AtlasServiceCollectionExtensions.AddAtlas"/> registers <see cref="AtlasOptions"/>.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterAtlasOptions()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AtlasOptions>>();

        Assert.Equal("Atlas", options.Value.Name);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasServiceCollectionExtensions.AddAtlas"/> binds <see cref="AtlasOptions"/>.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_BindAtlasConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Atlas:Name", "TestAtlas" }
            })
            .Build();

        var services = new ServiceCollection();

        services.AddAtlas(configuration);

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AtlasOptions>>();

        Assert.Equal("TestAtlas", options.Value.Name);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasServiceCollectionExtensions.AddAtlas"/> registers <see cref="IAtlasApplicationContext"/>.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterApplicationContext()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Atlas:Name", "TestAtlas" }
            }).Build();

        var services = new ServiceCollection();

        services.AddAtlas(configuration);

        using var provider = services.BuildServiceProvider();

        var context = provider.GetRequiredService<IAtlasApplicationContext>();

        Assert.Equal("TestAtlas", context.Name);
        Assert.NotEqual(Guid.Empty, context.InstanceId);
    }
}
