using Atlas.Abstractions.Configuration;
using Atlas.Abstractions.Events;
using Microsoft.Extensions.Options;

namespace Atlas.Hosting.Startup;

/// <summary>
/// Handles Atlas application startup events.
/// </summary>
public sealed class StartupHandler(
    IOptions<AtlasOptions> options) : IAtlasEventHandler<ApplicationStartedEvent>
{
    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationStartedEvent @event,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"{options.Value.Name} started at {@event.OccurredAt:O}");

        return Task.CompletedTask;
    }
}