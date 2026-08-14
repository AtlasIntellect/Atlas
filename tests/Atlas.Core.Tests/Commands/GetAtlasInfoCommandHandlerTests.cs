using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Runtime;
using Atlas.Core.Commands;
using Xunit;

namespace Atlas.Core.Tests.Commands;

/// <summary>
/// Provides unit tests for the <see cref="GetAtlasInfoCommandHandler"/> class.
/// </summary>
public sealed class GetAtlasInfoCommandHandlerTests
{
    /// <summary>
    /// Verifies that the handler returns the current Atlas instance information.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_ReturnAtlasInfo()
    {
        var instanceId = Guid.NewGuid();

        var applicationContext = new TestApplicationContext(
            "TestAtlas",
            instanceId);

        var handler = new GetAtlasInfoCommandHandler(
            applicationContext);

        var result = await handler.HandleAsync(
            new GetAtlasInfoCommand(),
            TestContext.Current.CancellationToken);

        Assert.Equal("TestAtlas", result.Name);
        Assert.Equal(instanceId, result.InstanceId);
    }

    private sealed class TestApplicationContext(
        string name,
        Guid instanceId) : IAtlasApplicationContext
    {
        public string Name => name;

        public Guid InstanceId => instanceId;
    }
}