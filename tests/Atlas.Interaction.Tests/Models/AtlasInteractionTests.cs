using Atlas.Interaction.Models;
using Xunit;

namespace Atlas.Interaction.Tests.Models;

/// <summary>
/// Provides unit tests for the <see cref="AtlasInteraction"/> class.
/// </summary>
public sealed class AtlasInteractionTests
{
    /// <summary>
    /// Verifies that a new interaction receives a unique identifier.
    /// </summary>
    [Fact]
    public void Constructor_Should_GenerateId()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        Assert.NotEqual(
            Guid.Empty,
            interaction.Id);
    }

    /// <summary>
    /// Verifies that a new interaction receives a creation timestamp.
    /// </summary>
    [Fact]
    public void Constructor_Should_SetCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;

        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var after = DateTimeOffset.UtcNow;

        Assert.InRange(
            interaction.CreatedAt,
            before,
            after);
    }

    /// <summary>
    /// Verifies that the interaction preserves the supplied input.
    /// </summary>
    [Fact]
    public void Constructor_Should_PreserveInput()
    {
        const string input = "What camera did I buy?";

        var interaction = new AtlasInteraction
        {
            Input = input
        };

        Assert.Equal(
            input,
            interaction.Input);
    }
}