using Atlas.AI.DependencyInjection;
using Atlas.Commands.DependencyInjection;
using Atlas.Events.DependencyInjection;
using Atlas.Interaction.DependencyInjection;
using Atlas.Memory.DependencyInjection;
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
            .AddAtlasHosting(configuration)
            .AddAtlasAi();

        return services;
    }
}