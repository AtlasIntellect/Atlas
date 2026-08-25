namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Interprets Atlas interactions into structured representations.
/// </summary>
public interface IAtlasInteractionInterpreter
{
    /// <summary>
    /// Interprets the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to interpret.</param>
    /// <returns>The interpretation of the interaction.</returns>
    AtlasInteractionInterpretation Interpret(
        AtlasInteraction interaction);
}