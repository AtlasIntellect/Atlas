using Atlas.Memory.Storage;
using Xunit;

namespace Atlas.Memory.Tests.Storage;

/// <summary>
/// Provides tests for the <see cref="InMemoryAtlasMemoryStoreInitializer"/> class.
/// </summary>
public sealed class InMemoryAtlasMemoryStoreInitializerTests
{
    /// <summary>
    /// Verifies that in-memory initialization completes successfully.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_Should_CompleteSuccessfully()
    {
        var initializer =
            new InMemoryAtlasMemoryStoreInitializer();

        var exception = await Record.ExceptionAsync(() =>
            initializer.InitializeAsync(TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }
}
