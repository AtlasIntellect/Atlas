using Atlas.Commands.Interfaces;
using Atlas.Memory.Commands;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;

namespace Atlas.Memory.Handlers;

/// <summary>
/// Handles commands to update existing Atlas memories.
/// </summary>
/// <param name="memory">The Atlas memory service.</param>
public sealed class UpdateMemoryCommandHandler(IAtlasMemory memory)
    : IAtlasCommandHandler<UpdateMemoryCommand, AtlasMemoryEntry>
{
    /// <inheritdoc />
    public async Task<AtlasMemoryEntry> HandleAsync(
        UpdateMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await memory.GetAsync(command.MemoryId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"No memory exists with identifier '{command.MemoryId}'.");
        }

        var updated = new AtlasMemoryEntry
        {
            Id = existing.Id,
            Content = command.Content,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt,
            Type = command.Type,
            Interpretation = command.Data is null
                ? existing.Interpretation
                : new AtlasMemoryInterpretation
                {
                    Data = command.Data
                },
            LifecycleState = existing.LifecycleState
        };

        await memory.UpdateAsync(updated, cancellationToken);

        // Read back the persisted entry to return its actual UpdatedAt value.
        return await memory.GetAsync(
                   existing.Id,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   $"Memory '{existing.Id}' was not found after updating.");
    }
}
