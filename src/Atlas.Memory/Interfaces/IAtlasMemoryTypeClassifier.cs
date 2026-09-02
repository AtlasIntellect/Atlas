using Atlas.Memory.Models;

namespace Atlas.Memory.Interfaces;

/// <summary>
/// Classifies the type of a memory based on its content.
/// </summary>
public interface IAtlasMemoryTypeClassifier
{
    /// <summary>
    /// Determines the type of the specified memory content.
    /// </summary>
    /// <param name="content">The memory content to classify.</param>
    /// <returns>The detected memory type.</returns>
    AtlasMemoryType Classify(string content);
}