using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jaeger.Reporters
{
    /// <summary>
    /// <see cref="LoggingReporter"/> logs every span it's given.
    /// </summary>
    public class LoggingReporter : IReporter
    {
        private ILogger Logger { get; }

        public LoggingReporter(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory?.CreateLogger<LoggingReporter>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void Report(Span span)
        {
            Logger.LogInformation("Span reported: {span}", span);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{nameof(LoggingReporter)}(Logger={Logger})";
        }
    }
}
