using Atlas.Commands.Dispatchers;
using Atlas.Commands.Handlers;
using Atlas.Commands.Interfaces;
using Atlas.Commands.Models;
using Atlas.Interaction.Models;
using Atlas.Memory.Models;
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
        services
        .AddSingleton<IAtlasCommandDispatcher, AtlasCommandDispatcher>()

        .AddSingleton<GetAtlasInfoCommandHandler>()
        .AddSingleton<
            IAtlasCommandHandler<GetAtlasInfoCommand, AtlasInfo>>(
            provider =>
                provider.GetRequiredService<GetAtlasInfoCommandHandler>())
        .AddSingleton<IAtlasCommandHandlerBase>(
            provider =>
                provider.GetRequiredService<GetAtlasInfoCommandHandler>())

        .AddSingleton<StoreMemoryCommandHandler>()
        .AddSingleton<
            IAtlasCommandHandler<StoreMemoryCommand, AtlasMemoryEntry>>(
            provider =>
                provider.GetRequiredService<StoreMemoryCommandHandler>())
        .AddSingleton<IAtlasCommandHandlerBase>(
            provider =>
                provider.GetRequiredService<StoreMemoryCommandHandler>())

        .AddSingleton<GetMemoryCommandHandler>()
        .AddSingleton<
            IAtlasCommandHandler<GetMemoryCommand, AtlasMemoryEntry?>>(
            provider =>
                provider.GetRequiredService<GetMemoryCommandHandler>())
        .AddSingleton<IAtlasCommandHandlerBase>(
            provider =>
                provider.GetRequiredService<GetMemoryCommandHandler>())

        .AddSingleton<SearchMemoryCommandHandler>()
        .AddSingleton<
            IAtlasCommandHandler<
                SearchMemoryCommand,
                IReadOnlyList<AtlasMemoryEntry>>>(
            provider =>
                provider.GetRequiredService<SearchMemoryCommandHandler>())
        .AddSingleton<IAtlasCommandHandlerBase>(
            provider =>
                provider.GetRequiredService<SearchMemoryCommandHandler>())

        .AddSingleton<ProcessInteractionCommandHandler>()
        .AddSingleton<
            IAtlasCommandHandler<ProcessInteractionCommand, AtlasResponse>>(
            provider =>
                provider.GetRequiredService<ProcessInteractionCommandHandler>())
        .AddSingleton<IAtlasCommandHandlerBase>(
            provider =>
                provider.GetRequiredService<ProcessInteractionCommandHandler>());

        return services;
    }
}