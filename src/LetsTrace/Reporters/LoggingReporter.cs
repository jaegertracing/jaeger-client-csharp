using Microsoft.Extensions.Logging;

namespace LetsTrace.Reporters
{
    public class LoggingReporter : IReporter
    {
        private ILogger _logger { get; }
        
        public LoggingReporter(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose() {}

        public void Report(ILetsTraceSpan span) => _logger.LogInformation($"Reporting span {span}");
    }
}
