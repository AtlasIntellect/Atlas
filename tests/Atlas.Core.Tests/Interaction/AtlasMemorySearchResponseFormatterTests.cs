using Atlas.Abstractions.Memory;
using Atlas.Core.Interaction;
using Xunit;

namespace Atlas.Core.Tests.Interaction;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemorySearchResponseFormatter"/> class.
/// </summary>
public sealed class AtlasMemorySearchResponseFormatterTests
{
    /// <summary>
    /// Verifies that the formatter returns a default response when no memories exist.
    /// </summary>
    [Fact]
    public void Format_Should_ReturnNoMemoriesResponse_WhenNoMemoriesExist()
    {
        var formatter = new AtlasMemorySearchResponseFormatter();

        var response = formatter.Format([]);

        Assert.Equal(
            "I couldn't find any matching memories.",
            response.Content);
    }

    /// <summary>
    /// Verifies that the formatter returns the content of a single memory.
    /// </summary>
    [Fact]
    public void Format_Should_ReturnMemoryContent_WhenOneMemoryExists()
    {
        var formatter = new AtlasMemorySearchResponseFormatter();

        var memory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var response = formatter.Format([memory]);

        Assert.Equal(
            memory.Content,
            response.Content);
    }

    /// <summary>
    /// Verifies that the formatter places multiple memories on separate lines.
    /// </summary>
    [Fact]
    public void Format_Should_ReturnMemoriesOnSeparateLines_WhenMultipleMemoriesExist()
    {
        var formatter = new AtlasMemorySearchResponseFormatter();

        var firstMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var secondMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I still have the Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var response = formatter.Format(
            [firstMemory, secondMemory]);

        Assert.Equal(
            $"{firstMemory.Content}{Environment.NewLine}{secondMemory.Content}",
            response.Content);
    }
}