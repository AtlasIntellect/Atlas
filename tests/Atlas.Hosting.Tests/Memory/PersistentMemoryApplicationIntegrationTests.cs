using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Atlas.Testing.Persistence;
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
        using var environment =
            new PersistentMemoryTestEnvironment();

        var memoryEntry =
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Atlas persistent memory test",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Fact
            };

        await StoreMemoryAsync(
            environment,
            memoryEntry);

        var restoredMemory =
            await GetMemoryAsync(
                environment,
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

    /// <summary>
    /// Verifies that a memory stored by one Atlas application instance
    /// can be found by a newly created application instance through search.
    /// </summary>
    [Fact]
    public async Task Memory_Should_BeSearchableAfterApplicationRecreation()
    {
        using var environment =
            new PersistentMemoryTestEnvironment();

        var memoryEntry =
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Atlas persistent camera memory",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Fact
            };

        await StoreMemoryAsync(
            environment,
            memoryEntry);

        var results =
            await SearchMemoryAsync(
                environment,
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

    private static async Task StoreMemoryAsync(
        PersistentMemoryTestEnvironment environment,
        AtlasMemoryEntry memoryEntry)
    {
        var builder = environment.CreateBuilder();

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
        PersistentMemoryTestEnvironment environment,
        Guid id)
    {
        var builder = environment.CreateBuilder();

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
        PersistentMemoryTestEnvironment environment,
        string query)
    {
        var builder = environment.CreateBuilder();

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
}
