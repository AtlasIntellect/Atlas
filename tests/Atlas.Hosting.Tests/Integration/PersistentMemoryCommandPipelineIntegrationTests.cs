using Atlas.Commands.Interfaces;
using Atlas.Hosting.DependencyInjection;
using Atlas.Memory.Commands;
using Atlas.Memory.Models;
using Atlas.Testing.Persistence;
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
        using var environment =
            new PersistentMemoryTestEnvironment();

            var storeResult =
                await StoreMemoryAsync(
                    environment);

            var restoredMemory =
                await GetMemoryAsync(
                    environment,
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
                    environment,
                    "persistent command");

            var searchedMemory =
                Assert.Single(searchResults);

            Assert.Equal(
                storeResult.Id,
                searchedMemory.Id);
    }

    private static async Task<AtlasMemoryEntry> StoreMemoryAsync(
        PersistentMemoryTestEnvironment environment)
    {
        var builder = environment.CreateBuilder();

        using var host = builder.Build();

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
        PersistentMemoryTestEnvironment environment,
        Guid memoryId)
    {
        var builder = environment.CreateBuilder();

        using var host = builder.Build();

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
        PersistentMemoryTestEnvironment environment,
        string query)
    {
        var builder = environment.CreateBuilder();

        using var host = builder.Build();

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
}
