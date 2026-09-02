using Microsoft.Extensions.DependencyInjection;

namespace Atlas.AI.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas AI services.
/// </summary>
public static class AtlasAiServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas AI services.
    /// </summary>
    public static IServiceCollection AddAtlasAi(
        this IServiceCollection services)
    {
        return services;
    }
}