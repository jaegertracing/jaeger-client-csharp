using System;
using System.Threading;
using Jaeger.Core;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport;
using Jaeger.Transport.Thrift.Transport.Sender;
using NSubstitute;
using Xunit;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Tests.Transport
{
    public class JaegerHttpTransportTests
    {
        private readonly ISerialization _mockJaegerThriftSerialization;
        private readonly ISender _mockSender;
        private readonly JaegerThriftTransport _testingTransport;
        private readonly int _batchSize = 4;

        public JaegerHttpTransportTests()
        {
            _mockJaegerThriftSerialization = Substitute.For<ISerialization>();
            _mockSender = Substitute.For<ISender>();

            _testingTransport = new JaegerHttpTransport(new Uri("http://localhost:14268"), _batchSize, _mockSender, _mockJaegerThriftSerialization);
        }

        [Fact]
        public void Constructor_ShouldSetBatchSizeToDefaultIfPassedInIsZero()
        {
            var transport = Substitute.For<JaegerHttpTransport>(0, "host", 1234);

            Assert.Equal(JaegerHttpTransport.DefaultBatchSize, transport.BatchSize);
        }

        [Fact]
        public async void AppendAsync_ShouldFlushWhenItReachesTheBatchSize()
        {
            var span = Substitute.For<IJaegerCoreSpan>();
            var tracer = Substitute.For<IJaegerCoreTracer>();
            span.Tracer.Returns(tracer);
            var cts = new CancellationTokenSource();

            var jProcess = Substitute.For<JaegerProcess>();
            _mockJaegerThriftSerialization.BuildJaegerProcessThrift(Arg.Is(tracer)).Returns(jProcess);

            var jSpan = Substitute.For<JaegerSpan>();
            _mockJaegerThriftSerialization.BuildJaegerThriftSpan(Arg.Is(span)).Returns(jSpan);

            _mockSender.BufferItem(Arg.Is(jSpan)).Returns(_batchSize);
            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Returns(_batchSize);

            var sent = await _testingTransport.AppendAsync(span, cts.Token);

            Assert.Equal(_batchSize, sent);
            _mockJaegerThriftSerialization.Received(1).BuildJaegerProcessThrift(Arg.Is(tracer));
            _mockJaegerThriftSerialization.Received(1).BuildJaegerThriftSpan(Arg.Is(span));
            _mockSender.Received(1).BufferItem(Arg.Is(jSpan));
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }
    }
}

