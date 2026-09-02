using Atlas.Commands.Interfaces;

namespace Atlas.Commands.Models;

/// <summary>
/// Represents a command that can be dispatched within the Atlas framework.
/// </summary>
public abstract record AtlasCommand : IAtlasCommand
{
    /// <summary>
    /// Gets the unique identifier of the command.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp indicating when the command was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}