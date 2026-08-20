using Atlas.Abstractions.Memory;

namespace Atlas.Core.Memory;

/// <summary>
/// Provides the default implementation for classifying memory content.
/// </summary>
public sealed class AtlasMemoryTypeClassifier : IAtlasMemoryTypeClassifier
{
    /// <inheritdoc/>
    public AtlasMemoryType Classify(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return AtlasMemoryType.Fact;
    }
}