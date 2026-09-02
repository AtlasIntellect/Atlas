using Atlas.Memory.Interfaces;

namespace Atlas.Memory.Models;

/// <summary>
/// Represents Atlas's structered interpretation of a memory.
/// </summary>
public sealed class AtlasMemoryInterpretation
{
    /// <summary>
    /// Gets the structured semantic data associated with the memory.
    /// </summary>
    public required IAtlasMemoryData Data { get; init; }
}
