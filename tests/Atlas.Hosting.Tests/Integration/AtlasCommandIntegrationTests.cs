using Atlas.Abstractions.Commands;
using Atlas.Core.Commands;
using Atlas.Hosting.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Atlas.Abstractions.Memory;
using Xunit;

namespace Atlas.Hosting.Tests.Integration;

/// <summary>
/// Provides integration tests for the Atlas command pipeline.
/// </summary>
public sealed class AtlasCommandIntegrationTests
{
    /// <summary>
    /// Verifies that Atlas can dispatch a command through the registered
    /// command dispatcher and handler.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_ExecuteRegisteredGetAtlasInfoCommand()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var provider = services.BuildServiceProvider();

        var dispatcher =
            provider.GetRequiredService<IAtlasCommandDispatcher>();

        var result =
            await dispatcher.DispatchAsync<GetAtlasInfoCommand, AtlasInfo>(
                new GetAtlasInfoCommand(),
                TestContext.Current.CancellationToken);

        Assert.Equal("Atlas", result.Name);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
    }

    /// <summary>
    /// Verifies that dispatching a <see cref="StoreMemoryCommand"/> through the host
    /// executes the registered handler and returns a valid memory entry.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_Should_ExecuteRegisteredStoreMemoryCommand()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddAtlas(builder.Configuration);

        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);

        var dispatcher =
            host.Services.GetRequiredService<IAtlasCommandDispatcher>();

        var content = "Test memory content";

        var entry = await dispatcher.DispatchAsync<StoreMemoryCommand, AtlasMemoryEntry>(
            new StoreMemoryCommand(content),
            TestContext.Current.CancellationToken);

        Assert.NotNull(entry);
        Assert.Equal(content, entry.Content);
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.NotEqual(default, entry.CreatedAt);

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that <see cref="GetMemoryCommand"/> retrieves a memory that was
    /// previously stored through the <see cref="AtlasCommandDispatcher"/>.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DispatchAsync_Should_RetrievePreviouslyStoredMemory()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddAtlas();

        using var host = builder.Build();

        await host.StartAsync(
            TestContext.Current.CancellationToken);
        
        var dispatcher = host.Services
            .GetRequiredService<IAtlasCommandDispatcher>();

        var stored = await dispatcher.DispatchAsync<
            StoreMemoryCommand,
            AtlasMemoryEntry>(
            new StoreMemoryCommand("Integration test memory"),
            TestContext.Current.CancellationToken);
        
        var retrieved = await dispatcher.DispatchAsync<
            GetMemoryCommand,
            AtlasMemoryEntry?>(
            new GetMemoryCommand(stored.Id),
            TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);
        Assert.Equal(stored.Id, retrieved.Id);
        Assert.Equal(stored.Content, retrieved.Content);
        Assert.Equal(stored.CreatedAt, retrieved.CreatedAt);

        await host.StopAsync(TestContext.Current.CancellationToken);
    }
}