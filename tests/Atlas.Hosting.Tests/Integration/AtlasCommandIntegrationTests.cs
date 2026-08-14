using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Configuration;
using Atlas.Core.Commands;
using Atlas.Hosting.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
}