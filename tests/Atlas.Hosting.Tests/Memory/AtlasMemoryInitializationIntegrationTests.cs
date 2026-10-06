using Atlas.Memory.Storage;
using Atlas.Testing.Persistence;
using Microsoft.EntityFrameworkCore;
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
        using var environment =
            new PersistentMemoryTestEnvironment();

        using var host =
            environment.CreateBuilder().Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);

        Assert.True(
            File.Exists(environment.DatabasePath));

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
}
