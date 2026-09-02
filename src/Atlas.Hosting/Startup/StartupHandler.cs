using Atlas.Abstractions.Configuration;
using Atlas.Events.Interfaces;
using Atlas.Events.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Hosting.Startup;

/// <summary>
/// Handles Atlas application startup events.
/// </summary>
public sealed class StartupHandler(
    IOptions<AtlasOptions> options,
    ILogger<StartupHandler> logger)
    : IAtlasEventHandler<ApplicationStartedEvent>
{
    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationStartedEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "{Name} started at {OccurredAt:O}",
            options.Value.Name,
            @event.OccurredAt);

        return Task.CompletedTask;
    }
}