using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Structured;

/// <inheritdoc/>
public sealed class AtlasInteractionInterpretationParser
    : IAtlasInteractionInterpretationParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    /// <inheritdoc/>
    public AtlasInteractionInterpretation Parse(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var structuredInterpretation =
            JsonSerializer.Deserialize<AtlasStructuredInteractionInterpretation>(
                content,
                SerializerOptions)
            ?? throw new InvalidOperationException(
                "The language model returned an empty interaction interpretation.");

        return new AtlasInteractionInterpretation(
            structuredInterpretation.Intent,
            structuredInterpretation.Query,
            structuredInterpretation.MemoryContent);
    }
}