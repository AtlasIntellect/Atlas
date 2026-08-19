using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Configuration;
using Atlas.Abstractions.Events;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Abstractions.Runtime;
using Atlas.Core.Commands;
using Atlas.Core.Events;
using Atlas.Core.Interaction;
using Atlas.Core.Runtime;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Atlas.Memory;
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
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration used to bind Atlas options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAtlas(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddLogging();

        var optionsBuilder = services.AddOptions<AtlasOptions>();

        if (configuration is not null)
            optionsBuilder.Bind(configuration.GetSection("Atlas"));
        else
            optionsBuilder.Configure(options =>
            {
                options.Name = "Atlas";
            });

        services
            .AddSingleton<IAtlasEventDispatcher, AtlasEventDispatcher>()
            .AddSingleton<IAtlasCommandDispatcher, AtlasCommandDispatcher>()
            .AddSingleton<GetAtlasInfoCommandHandler>()
            .AddSingleton<IAtlasCommandHandler<GetAtlasInfoCommand, AtlasInfo>>(
                provider => provider.GetRequiredService<GetAtlasInfoCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider => provider.GetRequiredService<GetAtlasInfoCommandHandler>())
            .AddSingleton<IAtlasMemory, AtlasMemory>()
            .AddSingleton<StoreMemoryCommandHandler>()
            .AddSingleton<IAtlasCommandHandler<StoreMemoryCommand, AtlasMemoryEntry>>(
                provider => provider.GetRequiredService<StoreMemoryCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider => provider.GetRequiredService<StoreMemoryCommandHandler>())
            .AddSingleton<GetMemoryCommandHandler>()
            .AddSingleton<IAtlasCommandHandler<GetMemoryCommand, AtlasMemoryEntry?>>(
                provider => provider.GetRequiredService<GetMemoryCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider => provider.GetRequiredService<GetMemoryCommandHandler>())
            .AddSingleton<SearchMemoryCommandHandler>()
            .AddSingleton<
                IAtlasCommandHandler<SearchMemoryCommand, IReadOnlyList<AtlasMemoryEntry>>>(
                provider => provider.GetRequiredService<SearchMemoryCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider => provider.GetRequiredService<SearchMemoryCommandHandler>())
            .AddSingleton<IAtlasInteractionProcessor, AtlasInteractionProcessor>()
            .AddSingleton<ProcessInteractionCommandHandler>()
            .AddSingleton<
                IAtlasCommandHandler<ProcessInteractionCommand, AtlasResponse>>(
                provider =>
                    provider.GetRequiredService<ProcessInteractionCommandHandler>())
            .AddSingleton<IAtlasCommandHandlerBase>(
                provider =>
                    provider.GetRequiredService<ProcessInteractionCommandHandler>())
            .AddSingleton<IAtlasRuntime, AtlasRuntime>()
            .AddSingleton<IAtlasApplicationContext, AtlasApplicationContext>()
            .AddSingleton<IAtlasEventHandlerBase, StartupHandler>()
            .AddHostedService<AtlasRuntimeHostedService>();

        return services;
    }
}