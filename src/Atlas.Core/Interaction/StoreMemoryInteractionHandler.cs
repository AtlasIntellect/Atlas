using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Abstractions.Memory;
using Atlas.Core.Commands;

namespace Atlas.Core.Interaction;

/// <summary>
/// Handles interactions that request storing a memory.
/// </summary>
public sealed class StoreMemoryInteractionHandler(
    IAtlasCommandDispatcher commandDispatcher,
    IAtlasMemoryTypeClassifier typeClassifier,
    IAtlasMemoryInterpreter interpreter)
    : IAtlasInteractionHandler
{
    /// <inheritdoc/>
    public AtlasInteractionIntent Intent =>
        AtlasInteractionIntent.StoreMemory;

    /// <inheritdoc/>
    public async Task<AtlasResponse> HandleAsync(
        AtlasInteraction interaction,
        AtlasInteractionInterpretation interpretation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = interpretation.MemoryContent ?? throw new InvalidOperationException(
                "Store-memory interpretation did not contain memory content.");

        var type = typeClassifier.Classify(content);

        var data = interpreter.Interpret(
            content,
            type);

        await commandDispatcher.DispatchAsync<
            StoreMemoryCommand,
            AtlasMemoryEntry>(
            new StoreMemoryCommand(
                content,
                type,
                data),
            cancellationToken);

        return new AtlasResponse
        {
            Content = "Memory stored successfully."
        };
    }
}
