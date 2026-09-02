using Atlas.Abstractions.Configuration;
using Atlas.Events.Interfaces;
using Atlas.Events.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Hosting.Startup;

/// <summary>
/// Handles Atlas application shutdown events.
/// </summary>
/// <param name="options">The Atlas options.</param>
/// <param name="logger">The logger.</param>
public sealed class ShutdownHandler(
    IOptions<AtlasOptions> options,
    ILogger<ShutdownHandler> logger)
    : IAtlasEventHandler<ApplicationStoppingEvent>
{
    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationStoppingEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "{Name} stopping at {OccurredAt:O}",
            options.Value.Name,
            @event.OccurredAt);

        return Task.CompletedTask;
    }
}