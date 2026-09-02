using Atlas.Commands.Interfaces;
using Atlas.Runtime.Commands;
using Atlas.Runtime.Handlers;
using Atlas.Runtime.Interfaces;
using Atlas.Runtime.Models;
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
                AtlasApplicationContext>()
            .AddSingleton<GetAtlasInfoCommandHandler>()
            .AddSingleton<
                IAtlasCommandHandler<GetAtlasInfoCommand, AtlasInfo>>(
                provider =>
                    provider.GetRequiredService<GetAtlasInfoCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider =>
                    provider.GetRequiredService<GetAtlasInfoCommandHandler>());

        return services;
    }
}