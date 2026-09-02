using Atlas.Commands.Interfaces;
using Atlas.Interaction.Commands;
using Atlas.Interaction.Detectors;
using Atlas.Interaction.Extractors;
using Atlas.Interaction.Handlers;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Interpreters;
using Atlas.Interaction.Models;
using Atlas.Interaction.Processors;
using Atlas.Runtime.Commands;
using Atlas.Runtime.Handlers;
using Atlas.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Interaction.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas integration services.
/// </summary>
public static class AtlasInteractionServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas interaction services.
    /// </summary>
    public static IServiceCollection AddAtlasInteraction(
        this IServiceCollection services)
    {
        services
            .AddSingleton<
                IAtlasInteractionHandler,
                SearchMemoryInteractionHandler>()
            .AddSingleton<
                IAtlasInteractionHandler,
                StoreMemoryInteractionHandler>()
            .AddSingleton<
                IAtlasInteractionHandler,
                UnknownInteractionHandler>()
            .AddSingleton<
                IAtlasInteractionIntentDetector,
                AtlasInteractionIntentDetector>()
            .AddSingleton<
                IAtlasInteractionProcessor,
                AtlasInteractionProcessor>()
            .AddSingleton<
                IAtlasInteractionQueryExtractor,
                AtlasInteractionQueryExtractor>()
            .AddSingleton<
                IAtlasInteractionMemoryContentExtractor,
                AtlasInteractionMemoryContentExtractor>()
            .AddSingleton<
                IAtlasInteractionInterpreter,
                AtlasInteractionInterpreter>()
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