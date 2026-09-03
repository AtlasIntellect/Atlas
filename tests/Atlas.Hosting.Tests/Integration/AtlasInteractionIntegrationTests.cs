using Atlas.Hosting.DependencyInjection;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
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
            Input = "What camera do I have?"
        };

        var searchResponse = await processor.ProcessAsync(
            searchInteraction,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            memoryContent,
            searchResponse.Content);
    }

    /// <summary>
    /// Verifies that a natural-language memory search is converted into a
    /// meaningful memory query and returns the matching memory.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_SearchMemoryUsingNaturalLanguageQuery()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var provider =
            services.BuildServiceProvider();

        var memory =
            provider.GetRequiredService<IAtlasMemory>();

        var processor =
            provider.GetRequiredService<IAtlasInteractionProcessor>();

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "I bought a Canon EOS 350D camera.",
                CreatedAt = DateTimeOffset.UtcNow
            },
            TestContext.Current.CancellationToken);

        var interaction = new AtlasInteraction
        {
            Input = "What camera did I buy?"
        };

        var response = await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            "I bought a Canon EOS 350D camera.",
            response.Content);
    }

    /// <summary>
    /// Verifies that Atlas can store a memory through one interaction and
    /// retrieve it through a subsequent natural-language interaction.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_StoreAndRetrieveUsingNaturalLanguage()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var provider =
            services.BuildServiceProvider();

        var processor =
            provider.GetRequiredService<IAtlasInteractionProcessor>();

        var storeInteraction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D camera."
        };

        await processor.ProcessAsync(
            storeInteraction,
            TestContext.Current.CancellationToken);

        var searchInteraction = new AtlasInteraction
        {
            Input = "What camera did I buy?"
        };

        var response = await processor.ProcessAsync(
            searchInteraction,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            "I bought a Canon EOS 350D camera.",
            response.Content);
    }

    /// <summary>
    /// Verifies that all Atlas interaction handlers are registered with dependency injection.
    /// </summary>
    [Fact]
    public void AddAtlas_Should_RegisterAllInteractionHandlers()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        using var provider = services.BuildServiceProvider();

        var handlers =
            provider
                .GetServices<IAtlasInteractionHandler>()
                .ToList();

        Assert.Contains(
            handlers,
            handler => handler.Intent == AtlasInteractionIntent.SearchMemory);

        Assert.Contains(
            handlers,
            handler => handler.Intent == AtlasInteractionIntent.StoreMemory);

        Assert.Contains(
            handlers,
            handler => handler.Intent == AtlasInteractionIntent.Unknown);
    }

    /// <summary>
    /// Verifies that Atlas can store a memory and subsequently retrieve it
    /// through a natural-language interaction using the complete dependency
    /// injection pipeline.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_StoreAndRetrieveMemoryThroughFullPipeline()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var serviceProvider =
            services.BuildServiceProvider();

        var processor =
            serviceProvider.GetRequiredService<IAtlasInteractionProcessor>();

        var storeInteraction = new AtlasInteraction
        {
            Input = "Remember that I bought a Canon EOS 350D camera."
        };

        await processor.ProcessAsync(
            storeInteraction,
            TestContext.Current.CancellationToken);

        var searchInteraction = new AtlasInteraction
        {
            Input = "What camera did I buy?"
        };

        var response = await processor.ProcessAsync(
            searchInteraction,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "I bought a Canon EOS 350D camera.",
            response.Content);
    }

    /// <summary>
    /// Verifies that a classified memory retains its type through the
    /// complete dependency injection pipeline.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_PreserveMemoryTypeThroughFullPipeline()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var serviceProvider =
            services.BuildServiceProvider();

        var processor =
            serviceProvider.GetRequiredService<IAtlasInteractionProcessor>();

        var interaction = new AtlasInteraction
        {
            Input = "Remember that my favorite color is blue."
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var memory =
            serviceProvider
                .GetRequiredService<IAtlasMemory>();

        var results = await memory.SearchAsync(
            "favorite color",
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(results);

        Assert.Equal(
            "my favorite color is blue.",
            entry.Content);

        Assert.Equal(
            AtlasMemoryType.Preference,
            entry.Type);
    }

    /// <summary>
    /// Verifies that a classified task retains its type through the
    /// complete dependency injection pipeline.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_PreserveTaskTypeThroughFullPipeline()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var serviceProvider =
            services.BuildServiceProvider();

        var processor =
            serviceProvider.GetRequiredService<IAtlasInteractionProcessor>();

        var interaction = new AtlasInteraction
        {
            Input = "Remember that I need to buy milk."
        };

        await processor.ProcessAsync(
            interaction,
            TestContext.Current.CancellationToken);

        var memory =
            serviceProvider
                .GetRequiredService<IAtlasMemory>();

        var results = await memory.SearchAsync(
            "buy milk",
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(results);

        Assert.Equal(
            "I need to buy milk.",
            entry.Content);

        Assert.Equal(
            AtlasMemoryType.Task,
            entry.Type);
    }

    /// <summary>
    /// Verifies that a natural-language memory search is interpreted and processed
    /// through the complete interaction pipeline.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Should_InterpretAndProcessSearchInteractionThroughFullPipeline()
    {
        var services = new ServiceCollection();

        services.AddAtlas();

        await using var serviceProvider =
            services.BuildServiceProvider();

        var memory =
            serviceProvider.GetRequiredService<IAtlasMemory>();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var processor =
            serviceProvider.GetRequiredService<IAtlasInteractionProcessor>();

        var response = await processor.ProcessAsync(
            new AtlasInteraction
            {
                Input = "What camera do I have?"
            },
            TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Contains(
            "Canon EOS 350D camera",
            response.Content,
            StringComparison.OrdinalIgnoreCase);
    }
}