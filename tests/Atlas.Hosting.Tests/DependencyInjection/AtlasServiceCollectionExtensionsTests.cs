using Atlas.Abstractions.Configuration;
using Atlas.Events.Dispatchers;
using Atlas.Events.Interfaces;
using Atlas.Hosting.DependencyInjection;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Interpreters;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Storage;
using Atlas.Runtime;
using Atlas.Runtime.Interfaces;
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
            })
            .Build();

        var services = new ServiceCollection();

        services.AddAtlas(configuration);

        using var provider = services.BuildServiceProvider();

        var context = provider.GetRequiredService<IAtlasApplicationContext>();

        Assert.Equal("TestAtlas", context.Name);
        Assert.NotEqual(Guid.Empty, context.InstanceId);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasServiceCollectionExtensions.AddAtlas"/> registers <see cref="IAtlasMemory"/>.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterAtlasMemory()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        using var provider = services.BuildServiceProvider();

        var memory = provider.GetRequiredService<IAtlasMemory>();

        Assert.IsType<AtlasMemory>(memory);
    }

    /// <summary>
    /// Verifies that the deterministic interaction interpreter is selected by default.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterDeterministicInteractionInterpreter_ByDefault()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        using var provider = services.BuildServiceProvider();

        var interpreter =
            provider.GetRequiredService<IAtlasInteractionInterpreter>();

        Assert.IsType<AtlasInteractionInterpreter>(interpreter);
    }

    /// <summary>
    /// Verifies that the deterministic interaction interpreter is selected when configured.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterDeterministicInteractionInterpreter_WhenConfigured()
    {
        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {
                        "Atlas:Interaction:InterpreterMode",
                        "Deterministic"
                    }
                })
                .Build();

        var services = new ServiceCollection();

        services.AddAtlas(configuration);

        using var provider = services.BuildServiceProvider();

        var interpreter =
            provider.GetRequiredService<IAtlasInteractionInterpreter>();

        Assert.IsType<AtlasInteractionInterpreter>(interpreter);
    }

    /// <summary>
    /// Verifies that the language-model interaction interpreter is registered when configured.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterLanguageModelInteractionInterpreter_WhenConfigured()
    {
        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {
                        "Atlas:Interaction:InterpreterMode",
                        "LanguageModel"
                    }
                })
                .Build();

        var services = new ServiceCollection();

        services.AddAtlas(configuration);

        var descriptor =
            services.FirstOrDefault(
                service =>
                    service.ServiceType == typeof(IAtlasInteractionInterpreter));

        Assert.NotNull(descriptor);

        Assert.Equal(
            typeof(AtlasLanguageModelInteractionInterpreter),
            descriptor.ImplementationType);
    }

    /// <summary>
    /// Verifies that an unsupported interaction interpreter mode is rejected.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_Throw_WhenInterpreterModeIsInvalid()
    {
        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {
                        "Atlas:Interaction:InterpreterMode",
                        "SomethingElse"
                    }
                })
                .Build();

        var services = new ServiceCollection();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => services.AddAtlas(configuration));

        Assert.Equal(
            "Unsupported Atlas interaction interpreter mode: 'SomethingElse'.",
            exception.Message);
    }
}
