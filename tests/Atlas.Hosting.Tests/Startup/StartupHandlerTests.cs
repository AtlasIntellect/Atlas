using Atlas.Abstractions.Configuration;
using Atlas.Abstractions.Events;
using Atlas.Hosting.Startup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Atlas.Hosting.Tests.Startup;

/// <summary>
/// Provides unit tests for the <see cref="StartupHandler"/> class.
/// </summary>
public sealed class StartupHandlerTests
{
    /// <summary>
    /// Verifies that the startup handler logs the Atlas startup message.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_LogAtlasStartedMessage()
    {
        var logger = new TestLogger<StartupHandler>();
        var options = Options.Create(new AtlasOptions
        {
            Name = "Atlas"
        });

        var handler = new StartupHandler(options, logger);

        var @event = new ApplicationStartedEvent
        {
            InstanceId = Guid.NewGuid()
        };

        await handler.HandleAsync(@event, TestContext.Current.CancellationToken);

        Assert.Contains(
            logger.Messages,
            message => message.Contains("Atlas started at"));
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
