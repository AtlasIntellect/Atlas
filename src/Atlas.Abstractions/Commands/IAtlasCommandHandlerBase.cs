namespace Atlas.Abstractions.Commands;

/// <summary>
/// Represents the non-generic base contract for an Atlas command handler.
/// </summary>
public interface IAtlasCommandHandlerBase
{
    /// <summary>
    /// Gets the type of command handled by this handler.
    /// </summary>
    Type CommandType { get; }

    /// <summary>
    /// Gets the type of result produced by this handler.
    /// </summary>
    Type ResultType { get; }

    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation and containing the command result.
    /// </returns>
    Task<object?> HandleAsync(
        IAtlasCommand command,
        CancellationToken cancellationToken = default);
}