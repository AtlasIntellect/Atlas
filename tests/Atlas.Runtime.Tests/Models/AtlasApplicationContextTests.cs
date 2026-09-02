using Atlas.Abstractions.Configuration;
using Atlas.Runtime.Models;
using Microsoft.Extensions.Options;
using Xunit;

namespace Atlas.Runtime.Tests.Models;

/// <summary>
/// Provides unit tests for <see cref="AtlasApplicationContext"/>.
/// </summary>
public sealed class AtlasApplicationContextTests
{
    /// <summary>
    /// Verifies that the application context exposes the configured Atlas name.
    /// </summary>
    [Fact]
    public void Name_Should_ReturnConfiguredAtlasName()
    {
        var options = Options.Create(new AtlasOptions
        {
            Name = "TestAtlas"
        });

        var context = new AtlasApplicationContext(options);

        Assert.Equal("TestAtlas", context.Name);
    }

    /// <summary>
    /// Verifies that the application context generates a unique instance identifier.
    /// </summary>
    [Fact]
    public void InstanceId_Should_NotBeEmpty()
    {
        var options = Options.Create(new AtlasOptions());

        var context = new AtlasApplicationContext(options);

        Assert.NotEqual(Guid.Empty, context.InstanceId);
    }

    /// <summary>
    /// Verifies that each application context receives its own instance identifier.
    /// </summary>
    [Fact]
    public void InstanceId_Should_BeUniquePerContext()
    {
        var options = Options.Create(new AtlasOptions());

        var first = new AtlasApplicationContext(options);
        var second = new AtlasApplicationContext(options);

        Assert.NotEqual(first.InstanceId, second.InstanceId);
    }
}
