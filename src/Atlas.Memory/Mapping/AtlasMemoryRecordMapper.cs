using Atlas.Memory.Models;
using Atlas.Memory.Models.Persistence;
using System.Text.Json;

namespace Atlas.Memory.Mapping;

/// <summary>
/// Maps between Atlas memory models and their persisted representations.
/// </summary>
public static class AtlasMemoryRecordMapper
{
    /// <summary>
    /// Maps an Atlas memory entry to a persistence record.
    /// </summary>
    public static AtlasMemoryRecord ToRecord(
        AtlasMemoryEntry memory)
    {
        ArgumentNullException.ThrowIfNull(memory);

        var record =
            new AtlasMemoryRecord
            {
                Id = memory.Id,
                Content = memory.Content,
                CreatedAt = memory.CreatedAt,
                Type = memory.Type
            };

        if (memory.Interpretation is null)
        {
            return record;
        }

        ArgumentNullException.ThrowIfNull(
            memory.Interpretation.Data);

        switch (memory.Interpretation.Data)
        {
            case AtlasTaskData taskData:
                record.InterpretationType =
                    AtlasMemoryDataType.Task;

                record.InterpretationData =
                    JsonSerializer.Serialize(taskData);

                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported memory data type: " +
                    $"{memory.Interpretation.Data.GetType().FullName}");
        }

        return record;
    }

    /// <summary>
    /// Maps a persistence record to an Atlas memory entry.
    /// </summary>
    public static AtlasMemoryEntry ToMemory(
        AtlasMemoryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var interpretation =
            CreateInterpretation(record);

        return new AtlasMemoryEntry
        {
            Id = record.Id,
            Content = record.Content,
            CreatedAt = record.CreatedAt,
            Type = record.Type,
            Interpretation = interpretation
        };
    }

    private static AtlasMemoryInterpretation? CreateInterpretation(
        AtlasMemoryRecord record)
    {
        if (record.InterpretationType is null ||
            record.InterpretationType == AtlasMemoryDataType.None)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(record.InterpretationData))
        {
            throw new InvalidOperationException(
                "Persisted memory interpretation data is missing.");
        }

        return record.InterpretationType switch
        {
            AtlasMemoryDataType.Task =>
                new AtlasMemoryInterpretation
                {
                    Data =
                        JsonSerializer.Deserialize<AtlasTaskData>(
                            record.InterpretationData)
                    ?? throw new InvalidOperationException(
                        "Persisted task interpretation data could not be deserialized.")
                },

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported memory data type: " +
                    $"{record.InterpretationType}.")
        };
    }
}
