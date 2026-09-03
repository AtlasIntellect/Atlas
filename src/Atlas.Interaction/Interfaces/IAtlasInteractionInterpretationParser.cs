using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interfaces;

/// <summary>
/// Defines a contract for parsing structured language-model output
/// into Atlas interaction interpretations.
/// </summary>
public interface IAtlasInteractionInterpretationParser
{
    /// <summary>
    /// Parses structured language-model output.
    /// </summary>
    /// <param name="content">The structured model output.</param>
    /// <returns>The parsed Atlas interaction interpretation.</returns>
    AtlasInteractionInterpretation Parse(
        string content);
}
