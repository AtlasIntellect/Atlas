using Atlas.Commands.Interfaces;
using Atlas.Hosting.DependencyInjection;
using Atlas.Memory.Commands;
using Atlas.Memory.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Atlas.Hosting.Tests.Integration;

/// <summary>
/// Provides integration tests for the Atlas memory command pipeline
/// when persistent SQLite storage is configured.
/// </summary>
public sealed class PersistentMemoryCommandPipelineIntegrationTests
{
    /// <summary>
    /// Verifies that StoreMemory, GetMemory, and SearchMemory
    /// operate through the persistent command pipeline across
    /// application recreation.
    /// </summary>
    [Fact]
    public async Task MemoryCommands_Should_PersistAcrossApplicationRecreation()
    {
        var databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"atlas-memory-{Guid.NewGuid():N}.db");

        try
        {
            var storeResult =
                await StoreMemoryAsync(
                    databasePath);

            var restoredMemory =
                await GetMemoryAsync(
                    databasePath,
                    storeResult.Id);

            Assert.NotNull(restoredMemory);

            Assert.Equal(
                storeResult.Id,
                restoredMemory.Id);

            Assert.Equal(
                storeResult.Content,
                restoredMemory.Content);

            Assert.Equal(
                storeResult.CreatedAt,
                restoredMemory.CreatedAt);

            Assert.Equal(
                storeResult.Type,
                restoredMemory.Type);

            Assert.Equal(
                storeResult.Interpretation,
                restoredMemory.Interpretation);

            var searchResults =
                await SearchMemoryAsync(
                    databasePath,
                    "persistent command");

            var searchedMemory =
                Assert.Single(searchResults);

            Assert.Equal(
                storeResult.Id,
                searchedMemory.Id);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static async Task<AtlasMemoryEntry> StoreMemoryAsync(
        string databasePath)
    {
        var builder =
            CreateBuilder(databasePath);

        builder.Services.AddAtlas(
            builder.Configuration);

        using var host =
            builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        var dispatcher =
            host.Services.GetRequiredService<
                IAtlasCommandDispatcher>();

        var result =
            await dispatcher.DispatchAsync<
                StoreMemoryCommand,
                AtlasMemoryEntry>(
                new StoreMemoryCommand(
                    "Atlas persistent command memory",
                    AtlasMemoryType.Fact),
                TestContext.Current.CancellationToken);

        await host.StopAsync(
            TestContext.Current.CancellationToken);

        return result;
    }

    private static async Task<AtlasMemoryEntry?> GetMemoryAsync(
        string databasePath,
        Guid memoryId)
    {
        var builder =
            CreateBuilder(databasePath);

        builder.Services.AddAtlas(
            builder.Configuration);

        using var host =
            builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        var dispatcher =
            host.Services.GetRequiredService<
                IAtlasCommandDispatcher>();

        var result =
            await dispatcher.DispatchAsync<
                GetMemoryCommand,
                AtlasMemoryEntry?>(
                new GetMemoryCommand(memoryId),
                TestContext.Current.CancellationToken);

        await host.StopAsync(
            TestContext.Current.CancellationToken);

        return result;
    }

    private static async Task<
        IReadOnlyList<AtlasMemoryEntry>> SearchMemoryAsync(
        string databasePath,
        string query)
    {
        var builder =
            CreateBuilder(databasePath);

        builder.Services.AddAtlas(
            builder.Configuration);

        using var host =
            builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        var dispatcher =
            host.Services.GetRequiredService<
                IAtlasCommandDispatcher>();

        var result =
            await dispatcher.DispatchAsync<
                SearchMemoryCommand,
                IReadOnlyList<AtlasMemoryEntry>>(
                new SearchMemoryCommand(query),
                TestContext.Current.CancellationToken);

        await host.StopAsync(
            TestContext.Current.CancellationToken);

        return result;
    }

    private static HostApplicationBuilder CreateBuilder(
        string databasePath)
    {
        var builder =
            Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Atlas:Memory:StorageMode"] = "Sqlite",
                ["ConnectionStrings:AtlasMemory"] =
                    $"Data Source={databasePath};Pooling=False"
            });

        return builder;
    }

    private static void DeleteDatabaseFiles(
        string databasePath)
    {
        var files =
            new[]
            {
                databasePath,
                $"{databasePath}-shm",
                $"{databasePath}-wal"
            };

        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
