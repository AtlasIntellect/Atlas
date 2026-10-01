using Atlas.Hosting.DependencyInjection;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Atlas.Hosting.Tests.Memory;

/// <summary>
/// Provides integration tests for persistent Atlas memory across application lifetimes.
/// </summary>
public sealed class PersistentMemoryApplicationIntegrationTests
{
    /// <summary>
    /// Verifies that a memory stored by one Atlas application instance
    /// can be retrieved by a newly created application instance.
    /// </summary>
    [Fact]
    public async Task Memory_Should_SurviveApplicationRecreation()
    {
        var databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"atlas-memory-{Guid.NewGuid():N}.db");

        try
        {
            var memoryEntry =
                new AtlasMemoryEntry
                {
                    Id = Guid.NewGuid(),
                    Content = "Atlas persistent memory test",
                    CreatedAt = DateTimeOffset.UtcNow,
                    Type = AtlasMemoryType.Fact
                };

            await StoreMemoryAsync(
                databasePath,
                memoryEntry);

            var restoredMemory =
                await GetMemoryAsync(
                    databasePath,
                    memoryEntry.Id);

            Assert.NotNull(restoredMemory);

            Assert.Equal(
                memoryEntry.Id,
                restoredMemory.Id);

            Assert.Equal(
                memoryEntry.Content,
                restoredMemory.Content);

            Assert.Equal(
                memoryEntry.CreatedAt,
                restoredMemory.CreatedAt);

            Assert.Equal(
                memoryEntry.Type,
                restoredMemory.Type);

            Assert.Equal(
                memoryEntry.Interpretation,
                restoredMemory.Interpretation);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    /// <summary>
    /// Verifies that a memory stored by one Atlas application instance
    /// can be found by a newly created application instance through search.
    /// </summary>
    [Fact]
    public async Task Memory_Should_BeSearchableAfterApplicationRecreation()
    {
        var databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"atlas-memory-{Guid.NewGuid():N}.db");

        try
        {
            var memoryEntry =
                new AtlasMemoryEntry
                {
                    Id = Guid.NewGuid(),
                    Content = "Atlas persistent camera memory",
                    CreatedAt = DateTimeOffset.UtcNow,
                    Type = AtlasMemoryType.Fact
                };

            await StoreMemoryAsync(
                databasePath,
                memoryEntry);

            var results =
                await SearchMemoryAsync(
                    databasePath,
                    "camera");

            var restoredMemory =
                Assert.Single(results);

            Assert.Equal(
                memoryEntry.Id,
                restoredMemory.Id);

            Assert.Equal(
                memoryEntry.Content,
                restoredMemory.Content);

            Assert.Equal(
                memoryEntry.CreatedAt,
                restoredMemory.CreatedAt);

            Assert.Equal(
                memoryEntry.Type,
                restoredMemory.Type);

            Assert.Equal(
                memoryEntry.Interpretation,
                restoredMemory.Interpretation);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static async Task StoreMemoryAsync(
        string databasePath,
        AtlasMemoryEntry memoryEntry)
    {
        var builder = CreateBuilder(databasePath);

        builder.Services.AddAtlas(
            builder.Configuration);

        using var host = builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        var memory =
            host.Services.GetRequiredService<IAtlasMemory>();

        await memory.StoreAsync(
            memoryEntry,
            TestContext.Current.CancellationToken);

        await host.StopAsync(
            TestContext.Current.CancellationToken);
    }

    private static async Task<AtlasMemoryEntry?> GetMemoryAsync(
        string databasePath,
        Guid id)
    {
        var builder = CreateBuilder(databasePath);

        builder.Services.AddAtlas(
            builder.Configuration);

        using var host = builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        var memory =
            host.Services.GetRequiredService<IAtlasMemory>();

        var result =
            await memory.GetAsync(
                id,
                TestContext.Current.CancellationToken);

        await host.StopAsync(
            TestContext.Current.CancellationToken);

        return result;
    }

    private static async Task<IReadOnlyList<AtlasMemoryEntry>> SearchMemoryAsync(
        string databasePath,
        string query)
    {
        var builder = CreateBuilder(databasePath);

        builder.Services.AddAtlas(
            builder.Configuration);

        using var host = builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        var memory =
            host.Services.GetRequiredService<IAtlasMemory>();

        var result =
            await memory.SearchAsync(
                query,
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
