namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Defines a contract for extracting query strings from instances of <see cref="AtlasInteraction"/>.
/// </summary>
public interface IAtlasInteractionQueryExtractor
{
    /// <summary>
    /// Extracts a query string from the specified <see cref="AtlasInteraction"/> instance.
    /// </summary>
    /// <param name="interaction">The <see cref="AtlasInteraction"/> instance containing the input data for the query extraction.</param>
    /// <returns>A string representing the extracted query.</returns>
    string ExtractQuery(AtlasInteraction interaction);
}