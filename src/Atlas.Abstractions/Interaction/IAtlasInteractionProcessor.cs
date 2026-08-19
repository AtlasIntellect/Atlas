namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Defines a service capable of processing interactions with Atlas.
/// </summary>
public interface IAtlasInteractionProcessor
{
    /// <summary>
    /// Processes the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation and containing the
    /// response produced by Atlas.
    /// </returns>
    Task<AtlasResponse> ProcessAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default);
}