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

        input = RemoveConversationalPrefix(input);
        input = RemoveQuestionPrefix(input);
        input = RemoveQuestionMarker(input);

        return input
            .Trim()
            .TrimEnd('?', '.', '!');
    }

    private static string RemoveConversationalPrefix(
        string input)
    {
        var prefixes = new[]
        {
            "Can you tell me ",
            "Could you tell me ",
            "Tell me "
        };

        var prefix = prefixes.FirstOrDefault(
            candidate =>
                input.StartsWith(
                    candidate,
                    StringComparison.OrdinalIgnoreCase));

        return prefix is null
            ? input
            : input[prefix.Length..];
    }

    private static string RemoveQuestionPrefix(
        string input)
    {
        var prefixes = new[]
        {
            "What ",
            "Which "
        };

        var prefix = prefixes.FirstOrDefault(
            candidate =>
                input.StartsWith(
                    candidate,
                    StringComparison.OrdinalIgnoreCase));

        return prefix is null
            ? input
            : input[prefix.Length..];
    }

    private static string RemoveQuestionMarker(
        string input)
    {
        var markers = new[]
        {
            " did I buy",
            " do I own",
            " do I have",
            " did I own",
            " did I have",
            " I bought",
            " I own",
            " I have"
        };

        var marker = markers.FirstOrDefault(
            candidate =>
                input.Contains(
                    candidate,
                    StringComparison.OrdinalIgnoreCase));

        if (marker is null)
        {
            return input;
        }

        var index = input.IndexOf(
            marker,
            StringComparison.OrdinalIgnoreCase);

        return input.Remove(
            index,
            marker.Length);
    }
}