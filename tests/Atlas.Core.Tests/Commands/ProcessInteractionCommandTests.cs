using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Interaction;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

/// <summary>
/// Provides unit tests for the <see cref="ProcessInteractionCommand"/> class.
/// </summary>
public sealed class ProcessInteractionCommandTests
{
    /// <summary>
    /// Verifies that the command preserves the supplied interaction.
    /// </summary>
    [Fact]
    public void Constructor_Should_PreserveInteraction()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var command = new ProcessInteractionCommand(interaction);

        Assert.Same(
            interaction,
            command.Interaction);
    }

    /// <summary>
    /// Verifies that the command implements <see cref="IAtlasCommand"/>.
    /// </summary>
    [Fact]
    public void Command_Should_ImplementIAtlasCommand()
    {
        var interaction = new AtlasInteraction
        {
            Input = "Hello Atlas"
        };

        var command = new ProcessInteractionCommand(interaction);

        Assert.IsType<IAtlasCommand>(command, exactMatch: false);
    }
}