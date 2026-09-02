using Atlas.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Commands.Dispatchers;

/// <summary>
/// Provides functionality for dispatching Atlas commands to their registered handlers.
/// </summary>
public sealed class AtlasCommandDispatcher(
    IServiceProvider serviceProvider)
    : IAtlasCommandDispatcher
{
    /// <inheritdoc/>
    public async Task<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : IAtlasCommand
    {
        var handler =
            serviceProvider.GetService<
                IAtlasCommandHandler<TCommand, TResult>>();

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No handler registered for command type: '{typeof(TCommand).FullName}'.");
        }

        return await handler.HandleAsync(
            command,
            cancellationToken);
    }
}