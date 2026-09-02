using Atlas.Events.Dispatchers;
using Atlas.Events.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Events.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas event services.
/// </summary>
public static class AtlasEventsServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas event services.
    /// </summary>
    public static IServiceCollection AddAtlasEvents(
        this IServiceCollection services)
    {
        services.AddSingleton<
            IAtlasEventDispatcher,
            AtlasEventDispatcher>();

        return services;
    }
}