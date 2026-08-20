namespace Atlas.Abstractions.Interaction;

/// <summary>
/// Provides the default implementation for extracting queries from Atlas interactions.
/// </summary>
public sealed class AtlasInteractionQueryExtractor
    : IAtlasInteractionQueryExtractor
{
    /// <inheritdoc/>
    public string ExtractQuery(AtlasInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        var input = interaction.Input.Trim();

        if (input.StartsWith(
                "What ",
                StringComparison.OrdinalIgnoreCase))
        {
            input = input["What ".Length..];
        }
        else if (input.StartsWith(
                     "Which ",
                     StringComparison.OrdinalIgnoreCase))
        {
            input = input["Which ".Length..];
        }

        var markers = new[]
        {
            " did I buy",
            " do I own"
        };

        foreach (var marker in markers)
        {
            var index = input.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                input = input.Remove(index, marker.Length);
                break;
            }
        }

        return input
            .Trim()
            .TrimEnd('?', '.', '!');
    }
}