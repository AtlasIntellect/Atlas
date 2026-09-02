namespace Atlas.Commands.Interfaces;

/// <summary>
/// Represents a dispatcher for executing commands within the Atlas framework.
/// </summary>
public interface IAtlasCommandDispatcher
{
    /// <summary>
    /// Dispatches the specified command asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to dispatch.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the command.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation and containing the command result.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the specified command type.
    /// </exception>
    Task<TResult> DispatchAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : IAtlasCommand;
}