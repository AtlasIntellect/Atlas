using Atlas.Abstractions.Configuration;
using Atlas.Events.Interfaces;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Hosting.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Atlas hosting services.
/// </summary>
public static class AtlasHostingServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atlas hosting services.
    /// </summary>
    public static IServiceCollection AddAtlasHosting(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddLogging();

        var optionsBuilder =
            services.AddOptions<AtlasOptions>();

        if (configuration is not null)
        {
            optionsBuilder.Bind(
                configuration.GetSection("Atlas"));
        }
        else
        {
            optionsBuilder.Configure(
                options =>
                {
                    options.Name = "Atlas";
                });
        }

        services.AddSingleton<
            IAtlasEventHandlerBase,
            StartupHandler>();

        services.AddHostedService<AtlasRuntimeHostedService>();

        return services;
    }
}