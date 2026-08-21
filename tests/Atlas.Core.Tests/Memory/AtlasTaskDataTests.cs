using Atlas.Abstractions.Memory;
using Xunit;

namespace Atlas.Core.Tests.Memory;

/// <summary>
/// Provides tests for <see cref="AtlasTaskData"/>.
/// </summary>
public sealed class AtlasTaskDataTests
{
    /// <summary>
    /// Verifies that a new task is active by default.
    /// </summary>
    [Fact]
    public void NewTask_Should_BeActiveByDefault()
    {
        var task = new AtlasTaskData
        {
            Description = "Buy groceries."
        };

        Assert.Equal(
            AtlasTaskStatus.Active,
            task.Status);
    }

    /// <summary>
    /// Verifies that a task stores its description.
    /// </summary>
    [Fact]
    public void Task_Should_StoreDescription()
    {
        var task = new AtlasTaskData
        {
            Description = "Buy groceries."
        };

        Assert.Equal(
            "Buy groceries.",
            task.Description);
    }

    /// <summary>
    /// Verifies that a task can have a due date.
    /// </summary>
    [Fact]
    public void Task_Should_StoreDueDate()
    {
        var dueAt =
            new DateTimeOffset(
                2026,
                8,
                25,
                18,
                30,
                0,
                TimeSpan.Zero);

        var task = new AtlasTaskData
        {
            Description = "Buy groceries.",
            DueAt = dueAt
        };

        Assert.Equal(
            dueAt,
            task.DueAt);
    }

    /// <summary>
    /// Verifies that a task can be completed.
    /// </summary>
    [Fact]
    public void Task_Should_StoreCompletedStatus()
    {
        var completedAt = DateTimeOffset.UtcNow;

        var task = new AtlasTaskData
        {
            Description = "Buy groceries.",
            Status = AtlasTaskStatus.Completed,
            CompletedAt = completedAt
        };

        Assert.Equal(
            AtlasTaskStatus.Completed,
            task.Status);

        Assert.Equal(
            completedAt,
            task.CompletedAt);
    }

    /// <summary>
    /// Verifies that a task can be cancelled.
    /// </summary>
    [Fact]
    public void Task_Should_StoreCancelledStatus()
    {
        var task = new AtlasTaskData
        {
            Description = "Buy groceries.",
            Status = AtlasTaskStatus.Cancelled
        };

        Assert.Equal(
            AtlasTaskStatus.Cancelled,
            task.Status);
    }

    /// <summary>
    /// Verifies that a task can exist without a due date.
    /// </summary>
    [Fact]
    public void Task_Should_AllowMissingDueDate()
    {
        var task = new AtlasTaskData
        {
            Description = "Buy groceries."
        };

        Assert.Null(task.DueAt);
    }

    /// <summary>
    /// Verifies that an incomplete task does not require a completion timestamp.
    /// </summary>
    [Fact]
    public void Task_Should_AllowMissingCompletedAt()
    {
        var task = new AtlasTaskData
        {
            Description = "Buy groceries."
        };

        Assert.Null(task.CompletedAt);
    }

    /// <summary>
    /// Verifies that task data can be attached to a memory interpretation.
    /// </summary>
    [Fact]
    public void Memory_Should_SupportTaskInterpretation()
    {
        var taskData = new AtlasTaskData
        {
            Description = "Buy groceries."
        };

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I need to buy groceries.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Task,
            Interpretation = new AtlasMemoryInterpretation
            {
                Data = taskData
            }
        };

        var interpretation =
            Assert.IsType<AtlasMemoryInterpretation>(
                memory.Interpretation);

        var result =
            Assert.IsType<AtlasTaskData>(
                interpretation.Data);

        Assert.Equal(
            "Buy groceries.",
            result.Description);

        Assert.Equal(
            AtlasTaskStatus.Active,
            result.Status);
    }
}