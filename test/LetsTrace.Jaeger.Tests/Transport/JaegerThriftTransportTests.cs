using System;
using System.Threading;
using System.Threading.Tasks;
using LetsTrace.Exceptions;
using LetsTrace.Jaeger.Serialization;
using LetsTrace.Jaeger.Transport;
using LetsTrace.Jaeger.Transport.Sender;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Thrift.Protocols;
using Xunit;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace LetsTrace.Jaeger.Tests.Transport
{
    public class JaegerThriftTransportTests
    {
        private readonly ISerialization _mockJaegerThriftSerialization;
        private readonly ITProtocolFactory _mockProtocolFactory;
        private readonly ISender _mockSender;
        private readonly JaegerThriftTransport _testingTransport;
        private int _bufferSize = 4;

        public JaegerThriftTransportTests()
        {
            _mockJaegerThriftSerialization = Substitute.For<ISerialization>();
            _mockProtocolFactory = Substitute.For<ITProtocolFactory>();
            _mockSender = Substitute.For<ISender>();

            _testingTransport = Substitute.For<JaegerThriftTransport>(_mockProtocolFactory, _mockSender, _mockJaegerThriftSerialization, _bufferSize);
        }

        [Fact]
        public void Constructor_ShouldSetBufferSizeToDefaultIfPassedInIsZero()
        {
            var transport = Substitute.For<JaegerThriftTransport>(_mockProtocolFactory, _mockSender, _mockJaegerThriftSerialization, 0);

            Assert.Equal(JaegerThriftTransport.DefaultBufferSize, transport.BufferSize);
        }

        [Fact]
        public void Dispose_ShouldDisposeTheSender()
        {
            _testingTransport.Dispose();

            _mockSender.Received(1).Dispose();
        }

        [Fact]
        public async void AppendAsync_ShouldCallSerializationAndSender()
        {
            var span = Substitute.For<ILetsTraceSpan>();
            var tracer = Substitute.For<ILetsTraceTracer>();
            span.Tracer.Returns(tracer);
            var cts = new CancellationTokenSource();

            var jProcess = Substitute.For<JaegerProcess>();
            _mockJaegerThriftSerialization.BuildJaegerProcessThrift(Arg.Is(tracer)).Returns(jProcess);

            var jSpan = Substitute.For<JaegerSpan>();
            _mockJaegerThriftSerialization.BuildJaegerThriftSpan(Arg.Is(span)).Returns(jSpan);

            _mockSender.BufferItem(Arg.Is(jSpan)).Returns(2);

            var sent = await _testingTransport.AppendAsync(span, cts.Token);

            Assert.Equal(0, sent);
            _mockJaegerThriftSerialization.Received(1).BuildJaegerProcessThrift(Arg.Is(tracer));
            _mockJaegerThriftSerialization.Received(1).BuildJaegerThriftSpan(Arg.Is(span));
            _mockSender.Received(1).BufferItem(Arg.Is(jSpan));
            await _mockSender.Received(0).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void AppendAsync_ShouldFlushWhenOverTheBufferSize()
        {
            var span = Substitute.For<ILetsTraceSpan>();
            var tracer = Substitute.For<ILetsTraceTracer>();
            span.Tracer.Returns(tracer);
            var cts = new CancellationTokenSource();

            var jProcess = Substitute.For<JaegerProcess>();
            _mockJaegerThriftSerialization.BuildJaegerProcessThrift(Arg.Is(tracer)).Returns(jProcess);

            var jSpan = Substitute.For<JaegerSpan>();
            _mockJaegerThriftSerialization.BuildJaegerThriftSpan(Arg.Is(span)).Returns(jSpan);

            _mockSender.BufferItem(Arg.Is(jSpan)).Returns(5);
            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Returns(5);

            var sent = await _testingTransport.AppendAsync(span, cts.Token);

            Assert.Equal(5, sent);
            _mockJaegerThriftSerialization.Received(1).BuildJaegerProcessThrift(Arg.Is(tracer));
            _mockJaegerThriftSerialization.Received(1).BuildJaegerThriftSpan(Arg.Is(span));
            _mockSender.Received(1).BufferItem(Arg.Is(jSpan));
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void FlushAsync_ShouldCallSenderAndReturnSentCount()
        {
            var cts = new CancellationTokenSource();
            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Returns(3);

            var sent = await _testingTransport.FlushAsync(cts.Token);

            Assert.Equal(3, sent);
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void FlushAsync_ShouldThrowIfSenderThrows()
        {
            var cts = new CancellationTokenSource();
            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Throws(new Exception("exception, yo"));

            var ex = await Assert.ThrowsAsync<SenderException>(() => _testingTransport.FlushAsync(cts.Token));

            Assert.Equal(0, ex.DroppedSpans);
            Assert.Equal("Failed to flush spans.", ex.Message);
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void CloseAsync_ShouldCallSenderIfTokenHasNotBeenCancelled()
        {
            var cts = new CancellationTokenSource();
            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Returns(3);

            var sent = await _testingTransport.CloseAsync(cts.Token);

            Assert.Equal(3,sent);
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void CloseAsync_ShouldNotCallSenderIfTokenHasBeenCancelled()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(() => _testingTransport.CloseAsync(cts.Token));

            await _mockSender.Received(0).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

    }
}

