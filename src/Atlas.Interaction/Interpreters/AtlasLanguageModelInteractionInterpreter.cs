using Atlas.AI.Interfaces;
using Atlas.AI.Structured;
using Atlas.Interaction.Interfaces;
using Atlas.Interaction.Models;

namespace Atlas.Interaction.Interpreters;

/// <summary>
/// Provides functionality to interpret <see cref="AtlasInteraction"/> instances
/// using a structured language model.
/// </summary>
/// <param name="languageModel">The language model to use for interpretation.</param>
/// <param name="parser">The parser to use for parsing the interpretation results.</param>
public sealed class AtlasLanguageModelInteractionInterpreter(
    IAtlasStructuredLanguageModel languageModel,
    IAtlasInteractionInterpretationParser parser)
    : IAtlasInteractionInterpreter
{
    /// <summary>
    /// Interprets the given <see cref="AtlasInteraction"/> using the provided
    /// language model and parser.
    /// </summary>
    /// <param name="interaction">The interaction to interpret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The interpreted interaction and its confidence information.</returns>
    public async Task<AtlasInteractionInterpretationResult> InterpretAsync(
        AtlasInteraction interaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        cancellationToken.ThrowIfCancellationRequested();

        var request =
            new AtlasStructuredLanguageModelRequest(
                BuildPrompt(interaction),
                typeof(AtlasStructuredInteractionInterpretation));

        var response =
            await languageModel.GenerateAsync(
                request,
                cancellationToken);

        return parser.Parse(response.Content);
    }

    private static string BuildPrompt(
        AtlasInteraction interaction)
    {
        return $"""
                Interpret the following Atlas interaction.

                Interaction:
                {interaction.Input}

                Return a JSON object containing:
                - intent
                - query
                - memoryContent
                - confidence
                - isAmbiguous
                """;
    }
}
