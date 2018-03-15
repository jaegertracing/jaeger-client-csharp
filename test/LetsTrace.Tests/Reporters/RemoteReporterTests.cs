using System;
using System.Threading;
using System.Threading.Tasks;
using LetsTrace.Exceptions;
using LetsTrace.Metrics;
using LetsTrace.Reporters;
using LetsTrace.Transport;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace LetsTrace.Tests.Reporters
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
            var span = Substitute.For<ILetsTraceSpan>();

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
            var span = Substitute.For<ILetsTraceSpan>();
            var metrics = InMemoryMetricsFactory.Instance.CreateMetrics();

            transport.CloseAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(2));

            using (var reporter = new RemoteReporter.Builder(transport).WithMetrics(metrics).Build())
            {
                reporter.Report(span);
                reporter.Report(span);

                transport.Received(2).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            Assert.Equal(2, metrics.ReporterSuccess.Count);
        }

        [Fact]
        public void RemoteReporter_ShouldCallMetricsFactory()
        {
            var transport = Substitute.For<ITransport>();
            var metricsFactory = InMemoryMetricsFactory.Instance;

            using (var reporter = new RemoteReporter.Builder(transport).WithMetricsFactory(metricsFactory).Build())
            {
                Assert.IsType<InMemoryMetricsFactory.InMemoryElement>(reporter._metrics.ReporterSuccess);
            }
        }

        [Fact]
        public void RemoteReporter_ShouldCountReporterDropped()
        {
            var transport = Substitute.For<ITransport>();
            var span = Substitute.For<ILetsTraceSpan>();
            var metrics = InMemoryMetricsFactory.Instance.CreateMetrics();

            transport.AppendAsync(span, Arg.Any<CancellationToken>()).Throws(new SenderException(String.Empty, 1));

            using (var reporter = new RemoteReporter.Builder(transport).WithMetrics(metrics).Build())
            {
                reporter.Report(span);

                transport.Received(1).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            Assert.Equal(0, metrics.ReporterSuccess.Count);
            Assert.Equal(1, metrics.ReporterDropped.Count);
            Assert.Equal(0, metrics.ReporterFailure.Count);
        }

        [Fact]
        public void RemoteReporter_ShouldCountReporterFailure()
        {
            var transport = Substitute.For<ITransport>();
            var span = Substitute.For<ILetsTraceSpan>();
            var metrics = InMemoryMetricsFactory.Instance.CreateMetrics();

            transport.CloseAsync(Arg.Any<CancellationToken>()).Throws(new SenderException(String.Empty, 1));

            using (var reporter = new RemoteReporter.Builder(transport).WithMetrics(metrics).Build())
            {
                reporter.Report(span);

                transport.Received(1).AppendAsync(span, Arg.Any<CancellationToken>());
            }
            transport.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            Assert.Equal(0, metrics.ReporterSuccess.Count);
            Assert.Equal(0, metrics.ReporterDropped.Count);
            Assert.Equal(1, metrics.ReporterFailure.Count);
        }
    }
}
