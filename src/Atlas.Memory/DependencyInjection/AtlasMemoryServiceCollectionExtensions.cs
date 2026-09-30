using Atlas.Commands.Interfaces;
using Atlas.Memory.Classifiers;
using Atlas.Memory.Commands;
using Atlas.Memory.Handlers;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Interpreters;
using Atlas.Memory.Models;
using Atlas.Memory.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var configuredStorageMode = configuration?["Atlas:Memory:StorageMode"];

        var storageMode =
            string.IsNullOrWhiteSpace(configuredStorageMode)
                ? AtlasMemoryStorageMode.InMemory
                : configuredStorageMode.Trim() switch
                {
                    "InMemory" => AtlasMemoryStorageMode.InMemory,
                    "Sqlite" => AtlasMemoryStorageMode.Sqlite,

                    var value =>
                        throw new InvalidOperationException(
                            $"Unsupported Atlas memory storage mode: {value}.")
                };

        switch (storageMode)
        {
            case AtlasMemoryStorageMode.InMemory:
                services.AddSingleton<
                    IAtlasMemoryStore,
                    InMemoryAtlasMemoryStore>();
                break;

            case AtlasMemoryStorageMode.Sqlite:
                var connectionString =
                    configuration?
                        .GetConnectionString("AtlasMemory");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException(
                        "An AtlasMemory SQLite connection string is required when using SQLite storage.");

                services.AddDbContextFactory<AtlasMemoryDbContext>(
                    options =>
                        options.UseSqlite(connectionString));

                services.AddSingleton<
                    IAtlasMemoryStore,
                    EntityFrameworkAtlasMemoryStore>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported Atlas memory storage mode: {storageMode}.");
        }

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
