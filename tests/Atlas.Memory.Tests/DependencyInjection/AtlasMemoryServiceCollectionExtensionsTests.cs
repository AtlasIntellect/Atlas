using Atlas.Memory.DependencyInjection;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Atlas.Memory.Tests.DependencyInjection;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemoryServiceCollectionExtensions"/> class.
/// </summary>
public sealed class AtlasMemoryServiceCollectionExtensionsTests
{
    /// <summary>
    /// Verifies that in-memory storage is selected by default.
    /// </summary>
    [Fact]
    public void AddAtlasMemory_Should_RegisterInMemoryStorage_ByDefault()
    {
        var services =
            new ServiceCollection();

        services.AddAtlasMemory();

        var descriptor =
            services.FirstOrDefault(
                service =>
                    service.ServiceType == typeof(IAtlasMemoryStore));

        Assert.NotNull(descriptor);

        Assert.Equal(
            typeof(InMemoryAtlasMemoryStore),
            descriptor.ImplementationType);

        Assert.Equal(
            ServiceLifetime.Singleton,
            descriptor.Lifetime);
    }

    /// <summary>
    /// Verifies that in-memory storage is selected when explicitly configured.
    /// </summary>
    [Fact]
    public void AddAtlasMemory_Should_RegisterInMemoryStorage_WhenConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Atlas:Memory:StorageMode"] = "InMemory"
                }).Build();

        var services = new ServiceCollection();

        services.AddAtlasMemory(configuration);

        var descriptor = services.FirstOrDefault(service =>
            service.ServiceType == typeof(IAtlasMemoryStore));

        Assert.NotNull(descriptor);

        Assert.Equal(
            typeof(InMemoryAtlasMemoryStore),
            descriptor.ImplementationType);

        Assert.Equal(
            ServiceLifetime.Singleton,
            descriptor.Lifetime);
    }

    /// <summary>
    /// Verifies that SQLite storage is registered when configured.
    /// </summary>
    [Fact]
    public void AddAtlasMemory_Should_RegisterSqliteStorage_WhenConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Atlas:Memory:StorageMode"] = "Sqlite",
                    ["ConnectionStrings:AtlasMemory"] = "Data Source=atlas-test.db"
                }).Build();

        var services = new ServiceCollection();

        services.AddAtlasMemory(configuration);

        var storeDescriptor = services.FirstOrDefault(service =>
            service.ServiceType == typeof(IAtlasMemoryStore));

        Assert.NotNull(storeDescriptor);

        Assert.Equal(
            typeof(EntityFrameworkAtlasMemoryStore),
            storeDescriptor.ImplementationType);

        Assert.Equal(ServiceLifetime.Singleton, storeDescriptor.Lifetime);

        var dbContextDescriptor = services.FirstOrDefault(service =>
            service.ServiceType == typeof(DbContextOptions<AtlasMemoryDbContext>));

        Assert.NotNull(dbContextDescriptor);
    }

    /// <summary>
    /// Verifies that an unsupported storage mode is rejected.
    /// </summary>
    [Fact]
    public void AddAtlasMemory_Should_Throw_WhenStorageModeIsInvalid()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Atlas:Memory:StorageMode"] = "SomethingElse"
                }).Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddAtlasMemory(configuration));

        Assert.Equal(
            "Unsupported Atlas memory storage mode: SomethingElse.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that SQLite storage requires a connection string.
    /// </summary>
    [Fact]
    public void AddAtlasMemory_Should_Throw_WhenSqliteConnectionStringIsMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Atlas:Memory:StorageMode"] = "Sqlite"
                }).Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddAtlasMemory(configuration));

        Assert.Equal(
            "An AtlasMemory SQLite connection string is required when using SQLite storage.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that a configured storage mode is case-sensitive.
    /// </summary>
    [Fact]
    public void AddAtlasMemory_Should_Throw_WhenStorageModeUsesDifferentCasing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Atlas:Memory:StorageMode"] = "sqlite"
            }).Build();

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
            services.AddAtlasMemory(configuration));
    }
}
