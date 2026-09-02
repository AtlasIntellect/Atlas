using Atlas.Commands.Dispatchers;
using Atlas.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Commands.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas command services.
/// </summary>
public static class AtlasCommandsServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas command services.
    /// </summary>
    public static IServiceCollection AddAtlasCommands(
        this IServiceCollection services)
    {
        services.AddSingleton<
            IAtlasCommandDispatcher,
            AtlasCommandDispatcher>();

        return services;
    }
}