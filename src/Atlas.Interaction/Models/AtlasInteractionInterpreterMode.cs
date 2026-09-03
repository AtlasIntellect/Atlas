namespace Atlas.Interaction.Models;

/// <summary>
/// Specifies the implementation used to interpret <see cref="AtlasInteraction"/>.
/// </summary>
public enum AtlasInteractionInterpreterMode
{
    /// <summary>
    /// Uses deterministic interaction interpretation.
    /// </summary>
    Deterministic = 0,

    /// <summary>
    /// Uses a language model for interaction interpretation.
    /// </summary>
    LanguageModel = 1,
}
