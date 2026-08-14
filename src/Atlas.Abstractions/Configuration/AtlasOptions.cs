namespace Atlas.Abstractions.Configuration;

/// <summary>
/// Provides configuration options for the Atlas application.
/// </summary>
public sealed class AtlasOptions
{
    /// <summary>
    /// Gets or sets the name of the Atlas instance.
    /// </summary>
    public string Name { get; set; } = "Atlas";
}