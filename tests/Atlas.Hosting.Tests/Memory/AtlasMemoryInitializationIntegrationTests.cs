using Atlas.Hosting.DependencyInjection;
using Atlas.Memory.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Atlas.Hosting.Tests.Memory;

/// <summary>
/// Provides integration tests for Atlas memory initialization during host startup.
/// </summary>
public sealed class AtlasMemoryInitializationIntegrationTests
{
    /// <summary>
    /// Verifies that Atlas creates and migrates a fresh SQLite memory database
    /// during application startup.
    /// </summary>
    [Fact]
    public async Task StartAsync_Should_InitializeFreshSqliteMemoryStore()
    {
        var databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"atlas-memory-{Guid.NewGuid():N}.db");

        try
        {
            Assert.False(File.Exists(databasePath));

            var builder =
                Host.CreateApplicationBuilder();

            builder.Configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Atlas:Memory:StorageMode"] = "Sqlite",
                    ["ConnectionStrings:AtlasMemory"] =
                        $"Data Source={databasePath};Pooling=false"
                });

            builder.Services.AddAtlas(
                builder.Configuration);

            using var host =
                builder.Build();

            await host.StartAsync(
                TestContext.Current.CancellationToken);

            Assert.True(File.Exists(databasePath));

            var factory =
                host.Services.GetRequiredService<
                    IDbContextFactory<AtlasMemoryDbContext>>();

            await using var dbContext =
                await factory.CreateDbContextAsync(
                    TestContext.Current.CancellationToken);

            var appliedMigrations =
                await dbContext.Database.GetAppliedMigrationsAsync(
                    TestContext.Current.CancellationToken);

            Assert.Contains(
                appliedMigrations,
                migration => migration.EndsWith(
                    "_InitialCreate",
                    StringComparison.Ordinal));

            var pendingMigrations =
                await dbContext.Database.GetPendingMigrationsAsync(
                    TestContext.Current.CancellationToken);

            Assert.Empty(pendingMigrations);

            var memories =
                await dbContext.Memories
                    .ToListAsync(
                        TestContext.Current.CancellationToken);

            Assert.Empty(memories);

            await host.StopAsync(
                TestContext.Current.CancellationToken);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
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
