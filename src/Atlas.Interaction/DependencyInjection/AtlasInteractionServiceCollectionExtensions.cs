using Atlas.Commands.Interfaces;
using Atlas.Interaction.Commands;
using Atlas.Interaction.Detectors;
using Atlas.Interaction.Extractors;
using Atlas.Interaction.Handlers;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Interpreters;
using Atlas.Interaction.Models;
using Atlas.Interaction.Processors;
using Atlas.Interaction.Structured;
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
        this IServiceCollection services,
        AtlasInteractionInterpreterMode mode =
            AtlasInteractionInterpreterMode.Deterministic)
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
                IAtlasInteractionInterpretationParser,
                AtlasInteractionInterpretationParser>()
            .AddSingleton<ProcessInteractionCommandHandler>()
            .AddSingleton<
                IAtlasCommandHandler<ProcessInteractionCommand, AtlasResponse>>(
                provider =>
                    provider.GetRequiredService<ProcessInteractionCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider =>
                    provider.GetRequiredService<ProcessInteractionCommandHandler>());

        switch (mode)
        {
            case AtlasInteractionInterpreterMode.Deterministic:
                services.AddSingleton<
                    IAtlasInteractionInterpreter,
                    AtlasInteractionInterpreter>();
                break;

            case AtlasInteractionInterpreterMode.LanguageModel:
                services.AddSingleton<
                    IAtlasInteractionInterpreter,
                    AtlasLanguageModelInteractionInterpreter>();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mode),
                    mode,
                    "Unsupported interaction interpreter mode.");
        }

        return services;
    }
}