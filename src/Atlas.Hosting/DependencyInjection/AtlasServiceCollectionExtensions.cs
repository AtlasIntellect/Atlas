using Atlas.Abstractions.Events;
using Atlas.Abstractions.Runtime;
using Atlas.Core.Events;
using Atlas.Core.Runtime;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
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
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAtlas(
        this IServiceCollection services)
    {
        services
            .AddSingleton<IAtlasEventDispatcher, AtlasEventDispatcher>()
            .AddSingleton<IAtlasRuntime, AtlasRuntime>()
            .AddSingleton<IAtlasEventHandlerBase, StartupHandler>()
            .AddHostedService<AtlasRuntimeHostedService>();

        return services;
    }
}