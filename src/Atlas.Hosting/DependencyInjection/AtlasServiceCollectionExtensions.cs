using Atlas.Abstractions.Configuration;
using Atlas.Commands.DependencyInjection;
using Atlas.Events.DependencyInjection;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Atlas.Interaction.DependencyInjection;
using Atlas.Memory.Classifiers;
using Atlas.Memory.DependencyInjection;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Interpretators;
using Atlas.Runtime.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Hosting.DependencyInjection;

/// <summary>
/// Provides dependency injection extensions for Atlas.
/// </summary>
public static class AtlasServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Atlas runtime and its core services.
    /// </summary>
    public static IServiceCollection AddAtlas(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services
            .AddAtlasCommands()
            .AddAtlasEvents()
            .AddAtlasMemory()
            .AddAtlasInteraction()
            .AddAtlasRuntime()
            .AddAtlasHosting(configuration);

        return services;
    }
}