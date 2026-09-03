using Atlas.AI.Structured;
using Atlas.AI.Tests.TestImplementations;
using Xunit;

namespace Atlas.AI.Tests;

/// <summary>
/// Provides unit tests for the test structured language model implementation.
/// </summary>
public sealed class TestAtlasStructuredLanguageModelTests
{
    /// <summary>
    /// Verifies that a request is accepted and the configured response is returned.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_ReturnConfiguredResponse()
    {
        var expectedResponse =
            new AtlasStructuredLanguageModelResponse(
                """{"intent":"SearchMemory"}""");

        var model =
            new TestAtlasStructuredLanguageModel
            {
                Response = expectedResponse
            };

        var request =
            new AtlasStructuredLanguageModelRequest(
                "Interpret this interaction.",
                typeof(object));

        var response =
            await model.GenerateAsync(
                request,
                TestContext.Current.CancellationToken);

        Assert.Same(
            expectedResponse,
            response);
    }

    /// <summary>
    /// Verifies that the request is passed to the language model.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_ReceiveRequest()
    {
        var model = new TestAtlasStructuredLanguageModel();

        var request =
            new AtlasStructuredLanguageModelRequest(
                "Interpret this interaction.",
                typeof(string));

        await model.GenerateAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Same(
            request,
            model.ReceivedRequest);
    }

    /// <summary>
    /// Verifies that the cancellation token is passed to the language model.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_PassCancellationToken()
    {
        var model = new TestAtlasStructuredLanguageModel();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        await model.GenerateAsync(
            new AtlasStructuredLanguageModelRequest(
                "Interpret this interaction.",
                typeof(string)),
            cancellationToken);

        Assert.Equal(
            cancellationToken,
            model.ReceivedCancellationToken);
    }

    /// <summary>
    /// Verifies that an already-cancelled request is rejected.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_Throw_WhenCancellationRequested()
    {
        var model = new TestAtlasStructuredLanguageModel();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => model.GenerateAsync(
                new AtlasStructuredLanguageModelRequest(
                    "Interpret this interaction.",
                    typeof(string)),
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that a null request is rejected.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_Throw_WhenRequestIsNull()
    {
        var model = new TestAtlasStructuredLanguageModel();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => model.GenerateAsync(
                null!,
                TestContext.Current.CancellationToken));
    }
}
