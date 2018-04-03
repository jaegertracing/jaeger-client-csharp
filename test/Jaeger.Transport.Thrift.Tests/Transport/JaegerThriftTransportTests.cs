using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core;
using Jaeger.Core.Exceptions;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport;
using Jaeger.Transport.Thrift.Transport.Sender;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Tests.Transport
{
    public class JaegerThriftTransportTests
    {
        private readonly ISerialization _mockJaegerThriftSerialization;
        private readonly ISender _mockSender;
        private readonly JaegerThriftTransport _testingTransport;

        public JaegerThriftTransportTests()
        {
            _mockJaegerThriftSerialization = Substitute.For<ISerialization>();
            _mockSender = Substitute.For<ISender>();

            _testingTransport = Substitute.For<JaegerThriftTransport>(_mockSender, _mockJaegerThriftSerialization);
        }

        [Fact]
        public void Dispose_ShouldDisposeTheSender()
        {
            _testingTransport.Dispose();

            _mockSender.Received(1).Dispose();
        }

        [Fact]
        public async void AppendAsync_ShouldCallSerializationAndProtocolAppendLogicAsync()
        {
            var span = Substitute.For<IJaegerCoreSpan>();
            var tracer = Substitute.For<IJaegerCoreTracer>();
            span.Tracer.Returns(tracer);
            var cts = new CancellationTokenSource();

            var jProcess = Substitute.For<JaegerProcess>();
            _mockJaegerThriftSerialization.BuildJaegerProcessThrift(Arg.Is(tracer)).Returns(jProcess);

            var jSpan = Substitute.For<JaegerSpan>();
            _mockJaegerThriftSerialization.BuildJaegerThriftSpan(Arg.Is(span)).Returns(jSpan);

            //_mockSender.BufferItem(Arg.Is(jSpan)).Returns(2);

            _testingTransport.ProtocolAppendLogicAsync(Arg.Is(jSpan), Arg.Any<CancellationToken>()).Returns(2);

            var sent = await _testingTransport.AppendAsync(span, cts.Token);

            Assert.Equal(2, sent);
            _mockJaegerThriftSerialization.Received(1).BuildJaegerProcessThrift(Arg.Is(tracer));
            _mockJaegerThriftSerialization.Received(1).BuildJaegerThriftSpan(Arg.Is(span));
            //_mockSender.Received(1).BufferItem(Arg.Is(jSpan));
            await _testingTransport.Received(1).ProtocolAppendLogicAsync(Arg.Is(jSpan), Arg.Any<CancellationToken>());
            await _mockSender.Received(0).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
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

