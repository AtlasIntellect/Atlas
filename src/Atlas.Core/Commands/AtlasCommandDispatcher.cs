using Atlas.Abstractions.Commands;

namespace Atlas.Core.Commands;

/// <summary>
/// Provides functionality for disparching Atlas commands to their registered handlers.
/// </summary>
public sealed class AtlasCommandDispatcher : IAtlasCommandDispatcher
{
    private readonly Dictionary<Type, IAtlasCommandHandlerBase> _handlers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AtlasCommandDispatcher"/> class.
    /// </summary>
    /// <param name="handlers">The command handlers registered with the dispatcher.</param>
    public AtlasCommandDispatcher(
        IEnumerable<IAtlasCommandHandlerBase> handlers)
    {
        foreach (var handler in handlers)
        {
            _handlers[handler.CommandType] = handler;
        }
    }

    /// <inheritdoc/>
    public async Task<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : IAtlasCommand
    {
        if (!_handlers.TryGetValue(typeof(TCommand), out var handler))
        {
            throw new InvalidOperationException(
                $"No handler registered for command type: '{typeof(TCommand).FullName}'.");
        }

        if (handler.ResultType != typeof(TResult))
        {
            throw new InvalidOperationException(
                $"The registered handler for command type '{typeof(TCommand).FullName}' " +
                $"does not produce the expected result type '{typeof(TResult).FullName}'.");
        }

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return (TResult)result!;
    }
}