using Atlas.AI.DependencyInjection;
using Atlas.Commands.DependencyInjection;
using Atlas.Events.DependencyInjection;
using Atlas.Interaction.DependencyInjection;
using Atlas.Interaction.Models;
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
        var configuredMode =
            configuration?
                .GetSection("Atlas:Interaction")
                .GetValue<string>("InterpreterMode");

        var mode =
            string.IsNullOrWhiteSpace(configuredMode)
                ? AtlasInteractionInterpreterMode.Deterministic
                : configuredMode.Trim() switch
                {
                    "Deterministic" =>
                        AtlasInteractionInterpreterMode.Deterministic,

                    "LanguageModel" =>
                        AtlasInteractionInterpreterMode.LanguageModel,

                    var value =>
                        throw new InvalidOperationException(
                            $"Unsupported Atlas interaction interpreter mode: '{value}'.")
                };

        services
            .AddAtlasEvents()
            .AddAtlasCommands()
            .AddAtlasMemory()
            .AddAtlasInteraction(mode)
            .AddAtlasRuntime()
            .AddAtlasAi()
            .AddAtlasHosting(configuration);

        return services;
    }
}