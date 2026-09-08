using Atlas.Memory.Mapping;
using Atlas.Memory.Models;
using Atlas.Memory.Models.Persistence;
using Xunit;

namespace Atlas.Memory.Tests.Mapping;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemoryRecordMapper"/> class.
/// </summary>
public sealed class AtlasMemoryRecordMapperTests
{
    /// <summary>
    /// Verifies that basic memory fields are mapped to a persistence record.
    /// </summary>
    [Fact]
    public void ToRecord_Should_MapMemoryFields()
    {
        var memory = new AtlasMemoryEntry 
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        var record = AtlasMemoryRecordMapper.ToRecord(memory);

        Assert.Equal(memory.Id, record.Id);

        Assert.Equal(memory.Content, record.Content);

        Assert.Equal(memory.CreatedAt, record.CreatedAt);

        Assert.Equal(memory.Type, record.Type);

        Assert.Null(record.InterpretationType);

        Assert.Null(record.InterpretationData);
    }

    /// <summary>
    /// Verifies that task interpretation data is mapped and serialized.
    /// </summary>
    [Fact]
    public void ToRecord_Should_SerializeTaskInterpretation()
    {
        var taskData = new AtlasTaskData 
        {
            Description = "Buy groceries.",
            Status = AtlasTaskStatus.Active,
            DueAt = new DateTimeOffset(
                2026,
                9,
                10,
                18,
                30,
                0,
                TimeSpan.Zero)
        };

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I need to buy groceries.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Task,
            Interpretation = 
                new AtlasMemoryInterpretation
                {
                    Data = taskData
                }
        };

        var record = AtlasMemoryRecordMapper.ToRecord(memory);

        Assert.Equal(AtlasMemoryDataType.Task, record.InterpretationType);

        Assert.NotNull(record.InterpretationData);

        Assert.Contains("Buy groceries.", record.InterpretationData);

        Assert.Contains(
            "\"Status\":0",
            record.InterpretationData);
    }

    /// <summary>
    /// Verifies that a memory without interpretation data does not persist interpretation data.
    /// </summary>
    [Fact]
    public void ToRecord_Should_LeaveInterpretationDataEmpty_WhenInterpretationIsAbsent()
    {
        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        var record = AtlasMemoryRecordMapper.ToRecord(memory);

        Assert.Null(record.InterpretationType);

        Assert.Null(record.InterpretationData);
    }

    /// <summary>
    /// Verifies that basic persistence fields are mapped back to an Atlas memory entry.
    /// </summary>
    [Fact]
    public void ToMemory_Should_MapMemoryFields()
    {
        var record = new AtlasMemoryRecord
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        var memory = AtlasMemoryRecordMapper.ToMemory(record);

        Assert.Equal(record.Id, memory.Id);

        Assert.Equal(record.Content, memory.Content);

        Assert.Equal(record.CreatedAt, memory.CreatedAt);

        Assert.Equal(record.Type, memory.Type);

        Assert.Null(memory.Interpretation);
    }

    /// <summary>
    /// Verifies that persisted task interpretation data is deserialized correctly.
    /// </summary>
    [Fact]
    public void ToMemory_Should_DeserializeTaskInterpretation()
    {
        var dueAt = new DateTimeOffset(
            2026,
            9,
            10,
            18,
            30,
            0,
            TimeSpan.Zero);

        var taskData = new AtlasTaskData
        {
            Description = "Buy groceries.",
            Status = AtlasTaskStatus.Active,
            DueAt = dueAt
        };

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I need to buy groceries.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Task,
            Interpretation =
                new AtlasMemoryInterpretation
                {
                    Data = taskData
                }
        };

        var record = AtlasMemoryRecordMapper.ToRecord(memory);

        var result = AtlasMemoryRecordMapper.ToMemory(record);

        var resultInterpretation = Assert.IsType<AtlasMemoryInterpretation>(
            result.Interpretation);

        var resultTask = Assert.IsType<AtlasTaskData>(
            resultInterpretation.Data);

        Assert.Equal(taskData.Description, resultTask.Description);

        Assert.Equal(taskData.Status, resultTask.Status);

        Assert.Equal(taskData.DueAt, resultTask.DueAt);

        Assert.Equal(taskData.CompletedAt, resultTask.CompletedAt);
    }

    /// <summary>
    /// Verifies that a record without interpretation data maps to a memory without an interpretation.
    /// </summary>
    [Fact]
    public void ToMemory_Should_LeaveInterpretationNull_WhenInterpretationIsAbsent()
    {
        var record = new AtlasMemoryRecord
        {
            Id = Guid.NewGuid(),
            Content = "Test memory.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact,
            InterpretationType = null,
            InterpretationData = null
        };

        var memory = AtlasMemoryRecordMapper.ToMemory(record);

        Assert.Null(memory.Interpretation);
    }

    /// <summary>
    /// Verifies that a complete memory survives mapping to a record and back.
    /// </summary>
    [Fact]
    public void RoundTrip_Should_PreserveMemory()
    {
        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I need to buy groceries.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Task,
            Interpretation = new AtlasMemoryInterpretation
            {
                Data = new AtlasTaskData
                {
                    Description = "Buy groceries.",
                    Status = AtlasTaskStatus.Completed,
                    DueAt = new DateTimeOffset(
                        2026,
                        9,
                        10,
                        18,
                        30,
                        0,
                        TimeSpan.Zero),
                    CompletedAt = new DateTimeOffset(
                        2026,
                        9,
                        9,
                        17,
                        0,
                        0,
                        TimeSpan.Zero)
                }
            }
        };

        var record = AtlasMemoryRecordMapper.ToRecord(original);

        var result = AtlasMemoryRecordMapper.ToMemory(record);

        Assert.Equal(original.Id, result.Id);

        Assert.Equal(original.Content, result.Content);

        Assert.Equal(original.CreatedAt, result.CreatedAt);

        Assert.Equal(original.Type, result.Type);

        var originalInterpretation = Assert.IsType<AtlasMemoryInterpretation>(
            original.Interpretation);

        var resultInterpretation = Assert.IsType<AtlasMemoryInterpretation>(
            result.Interpretation);

        var originalTask = Assert.IsType<AtlasTaskData>(
            originalInterpretation.Data);

        var resultTask = Assert.IsType<AtlasTaskData>(
            resultInterpretation.Data);

        Assert.Equal(originalTask.Description, resultTask.Description);

        Assert.Equal(originalTask.Status, resultTask.Status);

        Assert.Equal(originalTask.DueAt, resultTask.DueAt);

        Assert.Equal(originalTask.CompletedAt, resultTask.CompletedAt);
    }

    /// <summary>
    /// Verifies that mapping a null memory throws.
    /// </summary>
    [Fact]
    public void ToRecord_Should_Throw_WhenMemoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => AtlasMemoryRecordMapper.ToRecord(null!));
    }

    /// <summary>
    /// Verifies that mapping a null persistence record throws.
    /// </summary>
    [Fact]
    public void ToMemory_Should_Throw_WhenRecordIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => AtlasMemoryRecordMapper.ToMemory(null!));
    }
}
