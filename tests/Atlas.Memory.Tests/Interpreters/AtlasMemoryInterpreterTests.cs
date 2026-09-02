using Atlas.Memory.Interpreters;
using Atlas.Memory.Models;
using Xunit;

namespace Atlas.Memory.Tests.Interpreters;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemoryInterpreter"/> class.
/// </summary>
public sealed class AtlasMemoryInterpreterTests
{
    /// <summary>
    /// Verifies that the <see cref="AtlasMemoryInterpreter.Interpret(string, AtlasMemoryType)"/> method
    /// throws an <see cref="ArgumentNullException"/> when the provided memory content is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void Interpret_Should_Throw_WhenContentIsNull()
    {
        var interpreter = new AtlasMemoryInterpreter();

        Assert.Throws<ArgumentNullException>(
            () => interpreter.Interpret(
                null!,
                AtlasMemoryType.Task));
    }

    /// <summary>
    /// Verifies that common task statements are interpreted as structured tasks.
    /// </summary>
    [Theory]
    [InlineData(
        "I need to buy groceries.",
        "Buy groceries.")]
    [InlineData(
        "I have to call the dentist.",
        "Call the dentist.")]
    [InlineData(
        "I must finish the Atlas tests.",
        "Finish the Atlas tests.")]
    [InlineData(
        "Don't forget to pick up the package.",
        "Pick up the package.")]
    [InlineData(
        "Remind me to buy milk.",
        "Buy milk.")]
    public void Interpret_Should_ReturnTaskData_ForTaskStatements(
        string content,
        string expectedDescription)
    {
        var interpreter = new AtlasMemoryInterpreter();

        var result =
            interpreter.Interpret(
                content,
                AtlasMemoryType.Task);

        var task =
            Assert.IsType<AtlasTaskData>(result);

        Assert.Equal(
            expectedDescription,
            task.Description);

        Assert.Equal(
            AtlasTaskStatus.Active,
            task.Status);

        Assert.Null(task.DueAt);
        Assert.Null(task.CompletedAt);
    }

    /// <summary>
    /// Verifies that task interpretation is not performed for non-task memory types.
    /// </summary>
    [Theory]
    [InlineData(AtlasMemoryType.Fact)]
    [InlineData(AtlasMemoryType.Preference)]
    [InlineData(AtlasMemoryType.Conversation)]
    public void Interpret_Should_ReturnNull_ForNonTaskTypes(
        AtlasMemoryType type)
    {
        var interpreter = new AtlasMemoryInterpreter();

        var result =
            interpreter.Interpret(
                "I need to buy groceries.",
                type);

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that unsupported task content does not produce an interpretation.
    /// </summary>
    [Fact]
    public void Interpret_Should_ReturnNull_WhenTaskCannotBeInterpreted()
    {
        var interpreter = new AtlasMemoryInterpreter();

        var result =
            interpreter.Interpret(
                "This task has an unknown structure.",
                AtlasMemoryType.Task);

        Assert.Null(result);
    }
}