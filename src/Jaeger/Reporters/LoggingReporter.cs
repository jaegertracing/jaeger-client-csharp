using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Util;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jaeger.Reporters
{
    /// <summary>
    /// <see cref="LoggingReporter"/> logs every span it's given.
    /// </summary>
    public class LoggingReporter : IReporter
    {
        private ILogger Logger { get; }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new JaegerSpanContractResolver()
        };

        public LoggingReporter(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory?.CreateLogger<LoggingReporter>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void Report(Span span)
        {
            Logger.LogInformation("Span reported: {span}", JsonConvert.SerializeObject(span, JsonSerializerSettings));
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
