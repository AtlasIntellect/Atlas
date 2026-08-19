namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Represents a response produced by Atlas for an interaction.
/// </summary>
public sealed record AtlasResponse
{
    /// <summary>
    /// Gets the unique identifier of the response.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp indicating when the response was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the response content produced by Atlas.
    /// </summary>
    public required string Content { get; init; }
}