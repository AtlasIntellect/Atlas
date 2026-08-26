using Atlas.Interaction.Formatters;
using Atlas.Interaction.Interfaces;
using Atlas.Memory.Classifiers;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Interpretators;
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
                IAtlasMemorySearchResponseFormatter,
                AtlasMemorySearchResponseFormatter>()
            .AddSingleton<
                IAtlasMemoryTypeClassifier,
                AtlasMemoryTypeClassifier>()
            .AddSingleton<
                IAtlasMemoryInterpreter,
                AtlasMemoryInterpreter>();

        return services;
    }
}