using Atlas.Abstractions.Runtime;
using Atlas.Core.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Runtime.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas runtime services.
/// </summary>
public static class AtlasRuntimeServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas runtime services.
    /// </summary>
    public static IServiceCollection AddAtlasRuntime(
        this IServiceCollection services)
    {
        services
            .AddSingleton<IAtlasRuntime, AtlasRuntime>()
            .AddSingleton<
                IAtlasApplicationContext,
                AtlasApplicationContext>();

        return services;
    }
}