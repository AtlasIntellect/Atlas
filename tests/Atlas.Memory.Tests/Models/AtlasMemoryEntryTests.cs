using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Xunit;

namespace Atlas.Memory.Tests.Models;

/// <summary>
/// Represents unit tests for the <see cref="AtlasMemoryEntry"/> class.
/// </summary>
public sealed class AtlasMemoryEntryTests
{
    /// <summary>
    /// Tests that the default value of the <see cref="AtlasMemoryEntry.Type"/> property is <see cref="AtlasMemoryType.Fact"/>.
    /// </summary>
    [Fact]
    public void Type_Should_DefaultToFact()
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(), Content = "Test memory", CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(AtlasMemoryType.Fact, entry.Type);
    }

    /// <summary>
    /// Tests that the <see cref="AtlasMemoryEntry.Type"/> property supports all defined values of the <see cref="AtlasMemoryType"/> enum.
    /// </summary>
    /// <param name="type"></param>
    [Theory]
    [InlineData(AtlasMemoryType.Fact)]
    [InlineData(AtlasMemoryType.Preference)]
    [InlineData(AtlasMemoryType.Task)]
    [InlineData(AtlasMemoryType.Conversation)]
    public void Type_Should_SupportAllDefinedValues(AtlasMemoryType type)
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(), Content = "Test memory", CreatedAt = DateTimeOffset.UtcNow, Type = type
        };

        Assert.Equal(type, entry.Type);
    }

    /// <summary>
    /// Tests that the <see cref="AtlasMemoryEntry.Interpretation"/> property can be set.
    /// </summary>
    [Fact]
    public void Interpretation_Should_BeSettable()
    {
        var interpretation = new AtlasMemoryInterpretation
        {
            Data = new AtlasMemoryDataMock() // Assuming AtlasMemoryDataMock is a concrete implementation of IAtlasMemoryData for testing.
        };

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow,
            Interpretation = interpretation
        };

        Assert.Equal(interpretation, entry.Interpretation);
        Assert.Equal(interpretation.Data, entry.Interpretation.Data);
    }
    
    /// <summary>
    /// Tests that the default value of the <see cref="AtlasMemoryEntry.LifecycleState"/> property is <see cref="AtlasMemoryLifecycleState.Active"/>.
    /// </summary>
    [Fact]
    public void LifecycleState_Should_DefaultToActive()
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(
            AtlasMemoryLifecycleState.Active,
            entry.LifecycleState);
    }

    /// <summary>
    /// Tests that the default value of the <see cref="AtlasMemoryEntry.UpdatedAt"/> property is null.
    /// </summary>
    [Fact]
    public void UpdatedAt_Should_DefaultToNull()
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Null(entry.UpdatedAt);
    }

    /// <summary>
    /// Tests that the <see cref="AtlasMemoryEntry.LifecycleState"/> property supports all defined values of the <see cref="AtlasMemoryLifecycleState"/> enum.
    /// </summary>
    /// <param name="state"></param>
    [Theory]
    [InlineData(AtlasMemoryLifecycleState.Active)]
    [InlineData(AtlasMemoryLifecycleState.Archived)]
    public void LifecycleState_Should_SupportAllDefinedValues(
        AtlasMemoryLifecycleState state)
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow,
            LifecycleState = state
        };

        Assert.Equal(
            state,
            entry.LifecycleState);
    }

    private sealed class AtlasMemoryDataMock : IAtlasMemoryData
    {
        // Add any properties or methods required by IAtlasMemoryData for testing.
        // For this example, we'll assume it's a simple placeholder.
    }
}
