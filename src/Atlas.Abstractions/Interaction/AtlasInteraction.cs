namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Represents an interaction with an Atlas instance.
/// </summary>
public sealed record AtlasInteraction
{
    /// <summary>
    /// Gets the unique identifier of the interaction.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the time at which the interaction was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the input provided to Atlas.
    /// </summary>
    public required string Input { get; init; }
}