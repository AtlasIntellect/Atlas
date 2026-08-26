namespace Atlas.Memory.Models;

/// <summary>
/// Defines the type of information represented by an Atlas memory.
/// </summary>
public enum AtlasMemoryType
{
    /// <summary>
    /// Represents factual information.
    /// </summary>
    Fact,

    /// <summary>
    /// Represents a user preference.
    /// </summary>
    Preference,

    /// <summary>
    /// Represents something that needs to be done.
    /// </summary>
    Task,

    /// <summary>
    /// Represents conversational context.
    /// </summary>
    Conversation
}