namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Represents the intent identified for an Atlas interaction.
/// </summary>
public enum AtlasInteractionIntent
{
    /// <summary>
    /// Represents an unknown intent for an Atlas interaction.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Represents an intent to search memory within the Atlas interaction context.
    /// </summary>
    SearchMemory = 1,
    
    /// <summary>
    /// Represents an intent to store memory.
    /// </summary>
    StoreMemory = 2
}