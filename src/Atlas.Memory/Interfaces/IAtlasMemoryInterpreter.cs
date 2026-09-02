using Atlas.Memory.Models;

namespace Atlas.Memory.Interfaces;

/// <summary>
/// Interprets memory content into structured semantic information.
/// </summary>
public interface IAtlasMemoryInterpreter
{
    /// <summary>
    /// Interprets the specified memory content.
    /// </summary>
    /// <param name="content">The extracted memory content.</param>
    /// <param name="type">The classified memory type.</param>
    /// <returns>
    /// Structured semantic information when the content can be interpreted;
    /// otherwise <see langword="null"/>.
    /// </returns>
    IAtlasMemoryData? Interpret(
        string content,
        AtlasMemoryType type);
}