using Atlas.AI.Models;
using Atlas.AI.Tests.TestImplementations;
using Xunit;

namespace Atlas.AI.Tests;

/// <summary>
/// Provides unit tests for the test language model implementation.
/// </summary>
public sealed class TestAtlasLanguageModelTests
{
    /// <summary>
    /// Verifies that a request is accepted and the configured response is returned.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_ReturnConfiguredResponse()
    {
        var expectedResponse =
            new AtlasLanguageModelResponse(
                "Hello from the test model.");

        var model =
            new TestAtlasLanguageModel
            {
                Response = expectedResponse
            };

        var request =
            new AtlasLanguageModelRequest(
                "Say hello.");

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
        var model = new TestAtlasLanguageModel();

        var request =
            new AtlasLanguageModelRequest(
                "What is Atlas?");

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
        var model = new TestAtlasLanguageModel();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        await model.GenerateAsync(
            new AtlasLanguageModelRequest(
                "Hello."),
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
        var model = new TestAtlasLanguageModel();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => model.GenerateAsync(
                new AtlasLanguageModelRequest(
                    "Hello."),
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that a null request is rejected.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_Throw_WhenRequestIsNull()
    {
        var model = new TestAtlasLanguageModel();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => model.GenerateAsync(
                null!,
                TestContext.Current.CancellationToken));
    }
}
