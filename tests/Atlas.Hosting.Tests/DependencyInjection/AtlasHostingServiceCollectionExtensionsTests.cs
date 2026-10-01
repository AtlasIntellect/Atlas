using Atlas.Hosting.DependencyInjection;
using Atlas.Hosting.Memory;
using Atlas.Hosting.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Atlas.Hosting.Tests.DependencyInjection;

/// <summary>
/// Provides unit tests for the <see cref="AtlasHostingServiceCollectionExtensions"/> class.
/// </summary>
public sealed class AtlasHostingServiceCollectionExtensionsTests
{
    /// <summary>
    /// Verifies that memory store initialization is registered before
    /// the Atlas runtime hosted service.
    /// </summary>
    [Fact]
    public void AddAtlasHosting_Should_RegisterMemoryInitializationBeforeRuntime()
    {
        var services = new ServiceCollection();

        services.AddAtlasHosting();

        var hostedServiceTypes =
            services
                .Where(service =>
                    service.ServiceType == typeof(IHostedService))
                .Select(service =>
                    service.ImplementationType)
                .ToList();

        Assert.Equal(2, hostedServiceTypes.Count);

        Assert.Equal(
            typeof(AtlasMemoryInitializationHostedService),
            hostedServiceTypes[0]);

        Assert.Equal(
            typeof(AtlasRuntimeHostedService),
            hostedServiceTypes[1]);
    }
}
