using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Atlas.Testing.Persistence;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Atlas.Hosting.Tests.Memory;

/// <summary>
/// Provides tests for the persistent-memory test infrastructure.
/// </summary>
public sealed class PersistentMemoryTestEnvironmentTests
{
    /// <summary>
    /// Verifies that separate persistent-memory test environments
    /// use isolated databases.
    /// </summary>
    [Fact]
    public async Task Environments_Should_IsolatePersistentMemory()
    {
        using var firstEnvironment =
            new PersistentMemoryTestEnvironment();

        using var secondEnvironment =
            new PersistentMemoryTestEnvironment();

        var memoryEntry = new AtlasMemoryEntry 
        {
            Id = Guid.NewGuid(),
            Content = "Memory from first test environment",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        using var firstHost =
            firstEnvironment.CreateBuilder().Build();

        await firstHost.StartAsync(
            TestContext.Current.CancellationToken);

        var firstMemory =
            firstHost.Services.GetRequiredService<IAtlasMemory>();

        await firstMemory.StoreAsync(
            memoryEntry,
            TestContext.Current.CancellationToken);

        await firstHost.StopAsync(
            TestContext.Current.CancellationToken);

        using var secondHost =
            secondEnvironment.CreateBuilder().Build();

        await secondHost.StartAsync(
            TestContext.Current.CancellationToken);

        var secondMemory =
            secondHost.Services.GetRequiredService<IAtlasMemory>();

        var result =
            await secondMemory.GetAsync(
                memoryEntry.Id,
                TestContext.Current.CancellationToken);

        Assert.Null(result);

        await secondHost.StopAsync(
            TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that each persistent-memory test environment
    /// receives a unique database path.
    /// </summary>
    [Fact]
    public void Environments_Should_UseUniqueDatabasePaths()
    {
        using var firstEnvironment =
            new PersistentMemoryTestEnvironment();

        using var secondEnvironment =
            new PersistentMemoryTestEnvironment();

        Assert.NotEqual(
            firstEnvironment.DatabasePath,
            secondEnvironment.DatabasePath);
    }
}
