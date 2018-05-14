using System;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Reporters
{
    public class CompositeReporterTests
    {
        [Fact]
        public void CompositeReporter_ShouldCallLoggerOnBoth()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger1 = Substitute.For<ILogger>();
            var logger2 = Substitute.For<ILogger>();

            loggerFactory.CreateLogger<LoggingReporter>().Returns(logger1, logger2);

            var reporter1 = new LoggingReporter(loggerFactory);
            var reporter2 = new LoggingReporter(loggerFactory);
            var reporter = new CompositeReporter(reporter1, reporter2);

            var tracer = new Tracer.Builder("service")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            tracer.BuildSpan("foo").Start().Finish();
            loggerFactory.Received(2).CreateLogger<LoggingReporter>();

            logger1.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception, string>>());
            logger2.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception, string>>());
        }
    }
}
