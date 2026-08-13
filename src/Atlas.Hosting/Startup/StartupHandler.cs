using Atlas.Abstractions.Events;

namespace Atlas.Hosting.Startup;

/// <summary>
/// Handles Atlas application startup events.
/// </summary>
public sealed class StartupHandler : IAtlasEventHandler<ApplicationStartedEvent>
{
    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationStartedEvent @event,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Atlas started at {@event.OccurredAt:O}");

        return Task.CompletedTask;
    }
}