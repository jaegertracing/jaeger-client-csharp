using System;
using LetsTrace.Reporters;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LetsTrace.Tests.Reporters
{
    public class LoggingReporterTests
    {
        [Fact]
        public void LoggingReporter_ShouldCallLogger()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();
            var span = Substitute.For<ILetsTraceSpan>();

            loggerFactory.CreateLogger<LoggingReporter>().Returns(logger);

            using (var reporter = new LoggingReporter(loggerFactory))
            {
                loggerFactory.Received(1).CreateLogger<LoggingReporter>();

                reporter.Report(span);
                logger.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception, string>>());
            }
        }
    }
}
