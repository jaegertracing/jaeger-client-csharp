using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jaeger.Core.Reporters
{
    public class LoggingReporter : IReporter
    {
        private ILogger Logger { get; }
        
        public LoggingReporter(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory?.CreateLogger<LoggingReporter>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void Dispose() {}

        public void Report(IJaegerCoreSpan span) => Logger.LogInformation($"Reporting span:\n {JsonConvert.SerializeObject(span, Formatting.Indented)}");
    }
}
