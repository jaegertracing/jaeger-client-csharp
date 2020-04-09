using System;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Reporters
{
    public class LoggingReporterTests
    {
        [Fact]
        public void LoggingReporter_ShouldCallLogger()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();

            loggerFactory.CreateLogger<LoggingReporter>().Returns(logger);

            var reporter = new LoggingReporter(loggerFactory);

            var tracer = new Tracer.Builder("service")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            tracer.BuildSpan("foo").Start().Finish();

            loggerFactory.Received(1).CreateLogger<LoggingReporter>();
            logger.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LoggingReporter_CanLogActiveSpan()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();

            loggerFactory.CreateLogger<LoggingReporter>().Returns(logger);

            var reporter = new LoggingReporter(loggerFactory);

            var tracer = new Tracer.Builder("service")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            tracer.BuildSpan("foo").StartActive(true).Dispose();

            loggerFactory.Received(1).CreateLogger<LoggingReporter>();
            logger.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception, string>>());
        }
    }
}
