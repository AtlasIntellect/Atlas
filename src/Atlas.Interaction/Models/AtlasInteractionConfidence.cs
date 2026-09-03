namespace Atlas.Interaction.Models;

/// <summary>
/// Represents the confidence level of an Atlas interaction interpretation.
/// </summary>
public enum AtlasInteractionConfidence
{
    /// <summary>
    /// Atlas has low confidence in the interpretation.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Atlas has medium confidence in the interpretation.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Atlas has high confidence in the interpretation.
    /// </summary>
    High = 2
}
