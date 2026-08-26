using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interfaces;

/// <summary>
/// Defines a contract for extracting memory content from an
/// <see cref="AtlasInteraction"/>.
/// </summary>
public interface IAtlasInteractionMemoryContentExtractor
{
    /// <summary>
    /// Extracts the actual memory content from the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction containing the memory request.</param>
    /// <returns>The content that should be stored as a memory.</returns>
    string ExtractContent(AtlasInteraction interaction);
}