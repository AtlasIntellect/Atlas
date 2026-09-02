using Atlas.Commands.Interfaces;
using Atlas.Memory.Classifiers;
using Atlas.Memory.Commands;
using Atlas.Memory.Handlers;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Interpreters;
using Atlas.Memory.Models;
using Atlas.Memory.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Memory.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas memory services.
/// </summary>
public static class AtlasMemoryServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas memory services.
    /// </summary>
    public static IServiceCollection AddAtlasMemory(
        this IServiceCollection services)
    {
        services
            .AddSingleton<IAtlasMemory, AtlasMemory>()
            .AddSingleton<
                IAtlasMemoryTypeClassifier,
                AtlasMemoryTypeClassifier>()
            .AddSingleton<
                IAtlasMemoryInterpreter,
                AtlasMemoryInterpreter>()
            .AddSingleton<StoreMemoryCommandHandler>()
            .AddSingleton<IAtlasCommandHandler<StoreMemoryCommand, AtlasMemoryEntry>>(
                provider =>
                    provider.GetRequiredService<StoreMemoryCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider =>
                    provider.GetRequiredService<StoreMemoryCommandHandler>())
            .AddSingleton<GetMemoryCommandHandler>()
            .AddSingleton<IAtlasCommandHandler<GetMemoryCommand, AtlasMemoryEntry?>>(
                provider =>
                    provider.GetRequiredService<GetMemoryCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider =>
                    provider.GetRequiredService<GetMemoryCommandHandler>())
            .AddSingleton<SearchMemoryCommandHandler>()
            .AddSingleton<IAtlasCommandHandler<
                SearchMemoryCommand,
                IReadOnlyList<AtlasMemoryEntry>>>(
                provider =>
                    provider.GetRequiredService<SearchMemoryCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider =>
                    provider.GetRequiredService<SearchMemoryCommandHandler>());

        return services;
    }
}