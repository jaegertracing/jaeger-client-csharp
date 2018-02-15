using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LetsTrace.Reporters
{
    public class LoggingReporter : IReporter
    {
        private ILogger _logger { get; }
        
        public LoggingReporter(ILogger<LoggingReporter> logger)
        {
            _logger = logger;
        }

        public void Dispose() {}

        public void Report(ILetsTraceSpan span) => _logger.LogInformation($"Reporting span:\n {JsonConvert.SerializeObject(span, Formatting.Indented)}");
    }
}
