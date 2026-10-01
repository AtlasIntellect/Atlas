using Atlas.Memory.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atlas.Memory.Tests.Storage;

/// <summary>
/// Provides tests for the <see cref="EntityFrameworkAtlasMemoryStoreInitializer"/> class.
/// </summary>
public sealed class EntityFrameworkAtlasMemoryStoreInitializerTests
{
    /// <summary>
    /// Verifies that initialization applies the initial database migration
    /// to a fresh SQLite database.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_Should_ApplyInitialMigration()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync(
            TestContext.Current.CancellationToken);

        var options =
            new DbContextOptionsBuilder<AtlasMemoryDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory =
            new TestDbContextFactory(options);

        var initializer =
            new EntityFrameworkAtlasMemoryStoreInitializer(factory);

        await initializer.InitializeAsync(
            TestContext.Current.CancellationToken);

        await using var dbContext =
            await factory.CreateDbContextAsync(
                TestContext.Current.CancellationToken);

        var tableExists =
            await TableExistsAsync(
                dbContext,
                "Memories",
                TestContext.Current.CancellationToken);

        Assert.True(tableExists);

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
    }

    /// <summary>
    /// Verifies that initialization can be run repeatedly
    /// without attempting to recreate the existing schema.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_Should_BeIdempotent()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync(
            TestContext.Current.CancellationToken);

        var options =
            new DbContextOptionsBuilder<AtlasMemoryDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory =
            new TestDbContextFactory(options);

        var initializer =
            new EntityFrameworkAtlasMemoryStoreInitializer(factory);

        await initializer.InitializeAsync(
            TestContext.Current.CancellationToken);

        await initializer.InitializeAsync(
            TestContext.Current.CancellationToken);

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
    }

    /// <summary>
    /// Verifies that initialization honors a cancelled cancellation token.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_Should_HonorCancellation()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync(
            TestContext.Current.CancellationToken);

        var options =
            new DbContextOptionsBuilder<AtlasMemoryDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory =
            new TestDbContextFactory(options);

        var initializer =
            new EntityFrameworkAtlasMemoryStoreInitializer(factory);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () =>
                initializer.InitializeAsync(
                    cancellationTokenSource.Token));
    }

    private static async Task<bool> TableExistsAsync(
        AtlasMemoryDbContext dbContext,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command =
            dbContext.Database.GetDbConnection().CreateCommand();

        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = $tableName;
            """;

        var parameter =
            command.CreateParameter();

        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;

        command.Parameters.Add(parameter);

        var result =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result) > 0;
    }

    private sealed class TestDbContextFactory(
        DbContextOptions<AtlasMemoryDbContext> options)
        : IDbContextFactory<AtlasMemoryDbContext>
    {
        public AtlasMemoryDbContext CreateDbContext()
        {
            return new AtlasMemoryDbContext(options);
        }

        public Task<AtlasMemoryDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                new AtlasMemoryDbContext(options));
        }
    }
}
