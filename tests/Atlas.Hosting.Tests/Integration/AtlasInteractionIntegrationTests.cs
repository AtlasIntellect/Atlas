using Atlas.Abstractions.Interaction;
using Atlas.Hosting.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Atlas.Hosting.Tests.Integration;

/// <summary>
/// Provides integration tests for Atlas interaction processing.
/// </summary>
public sealed class AtlasInteractionIntegrationTests
{
    /// <summary>
    /// Verifies that an interaction can store a memory that can subsequently
    /// be retrieved through a memory search interaction.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_StoreAndRetrieveMemory()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var provider =
            services.BuildServiceProvider();

        var processor =
            provider.GetRequiredService<IAtlasInteractionProcessor>();

        const string memoryContent =
            "I bought a Canon EOS 350D camera.";

        var storeInteraction = new AtlasInteraction
        {
            Input = $"Remember that {memoryContent}"
        };

        var storeResponse = await processor.ProcessAsync(
            storeInteraction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "Memory stored successfully.",
            storeResponse.Content);

        var searchInteraction = new AtlasInteraction
        {
            Input = "I bought a Canon EOS 350D camera."
        };

        var searchResponse = await processor.ProcessAsync(
            searchInteraction,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            memoryContent,
            searchResponse.Content);
    }
}