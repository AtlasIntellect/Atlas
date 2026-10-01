using Atlas.Memory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Memory.Storage;

/// <summary>
/// Initializes the Entity Framework Core Atlas memory store
/// and applies pending database migrations.
/// </summary>
/// <param name="dbContextFactory">
/// Factory used to create short-lived memory database contexts.
/// </param>
public sealed class EntityFrameworkAtlasMemoryStoreInitializer(
    IDbContextFactory<AtlasMemoryDbContext> dbContextFactory)
    : IAtlasMemoryStoreInitializer
{
    /// <inheritdoc />
    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        await dbContext.Database.MigrateAsync(
            cancellationToken);
    }
}
