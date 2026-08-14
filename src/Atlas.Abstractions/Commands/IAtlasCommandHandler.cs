namespace Atlas.Abstractions.Commands;

/// <summary>
/// Represents a handler for an Atlas command.
/// </summary>
/// <typeparam name="TCommand">The type of command handled by this handler.</typeparam>
/// <typeparam name="TResult">The type of result produced by this handler.</typeparam>
public interface IAtlasCommandHandler<in TCommand, TResult>
    : IAtlasCommandHandlerBase
    where TCommand : IAtlasCommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation and containing the command result.
    /// </returns>
    Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);

    Type IAtlasCommandHandlerBase.CommandType =>
        typeof(TCommand);

    Type IAtlasCommandHandlerBase.ResultType =>
        typeof(TResult);

    async Task<object?> IAtlasCommandHandlerBase.HandleAsync(
        IAtlasCommand command,
        CancellationToken cancellationToken)
    {
        return await HandleAsync(
            (TCommand)command,
            cancellationToken);
    }
}