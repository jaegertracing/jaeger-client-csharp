using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core.Exceptions;
using Jaeger.Core.Metrics;
using Jaeger.Core.Reporters;
using Jaeger.Core.Transport;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Jaeger.Core.Tests.Reporters
{
    public class RemoteReporterTests
    {
        [Fact]
        public void RemoteReporter_Constructor_ShouldThrowWhenTransportIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RemoteReporter.Builder(null).Build());
            Assert.Equal("transport", ex.ParamName);
        }

        [Fact]
        public void RemoteReporter_ShouldCallTransport()
        {
            var transport = Substitute.For<ITransport>();
            var span = Substitute.For<IJaegerCoreSpan>();

            using (var reporter = new RemoteReporter.Builder(transport).Build())
            {
                reporter.Report(span);
                transport.Received(1).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public void RemoteReporter_ShouldCallLogger()
        {
            var transport = Substitute.For<ITransport>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();

            loggerFactory.CreateLogger<RemoteReporter>().Returns(logger);

            using (new RemoteReporter.Builder(transport).WithLoggerFactory(loggerFactory).Build())
            {
                loggerFactory.Received(1).CreateLogger<RemoteReporter>();
            }
        }

        [Fact]
        public void RemoteReporter_ShouldCallMetrics()
        {
            var transport = Substitute.For<ITransport>();
            var span = Substitute.For<IJaegerCoreSpan>();
            var metrics = Substitute.For<IMetrics>();

            transport.CloseAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(2));

            using (var reporter = new RemoteReporter.Builder(transport).WithMetrics(metrics).Build())
            {
                reporter.Report(span);
                reporter.Report(span);

                transport.Received(2).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            metrics.ReporterSuccess.Received(1).Inc(2);
        }

        [Fact]
        public void RemoteReporter_ShouldCountReporterDropped()
        {
            var transport = Substitute.For<ITransport>();
            var span = Substitute.For<IJaegerCoreSpan>();
            var metrics = Substitute.For<IMetrics>();

            transport.AppendAsync(span, Arg.Any<CancellationToken>()).Throws(new SenderException(String.Empty, 1));

            using (var reporter = new RemoteReporter.Builder(transport).WithMetrics(metrics).Build())
            {
                reporter.Report(span);

                transport.Received(1).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            var _ = metrics.DidNotReceive().ReporterSuccess;
            _ = metrics.Received(1).ReporterDropped;
            _ = metrics.DidNotReceive().ReporterFailure;
        }

        [Fact]
        public void RemoteReporter_ShouldCountReporterFailure()
        {
            var transport = Substitute.For<ITransport>();
            var span = Substitute.For<IJaegerCoreSpan>();
            var metrics = Substitute.For<IMetrics>();

            transport.CloseAsync(Arg.Any<CancellationToken>()).Throws(new SenderException(String.Empty, 1));

            using (var reporter = new RemoteReporter.Builder(transport).WithMetrics(metrics).Build())
            {
                reporter.Report(span);

                transport.Received(1).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            var _ = metrics.DidNotReceive().ReporterSuccess;
            _ = metrics.DidNotReceive().ReporterDropped;
            _ = metrics.Received(1).ReporterFailure;
        }
    }
}
