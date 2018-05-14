using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jaeger.Core.Reporters
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
            Logger.LogInformation("Span reported: {span}", JsonConvert.SerializeObject(span));
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
