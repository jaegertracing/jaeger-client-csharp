using System;
using System.Collections.Generic;
using System.Threading;
using Jaeger.Core;
using Jaeger.Thrift;
using Jaeger.Thrift.Agent;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport;
using Jaeger.Transport.Thrift.Transport.Internal;
using Jaeger.Transport.Thrift.Transport.Sender;
using NSubstitute;
using Thrift.Protocols;
using Xunit;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;
using JaegerTag = Jaeger.Thrift.Tag;
using JaegerBatch = Jaeger.Thrift.Batch;

namespace Jaeger.Transport.Thrift.Tests.Transport
{
    public class JaegerUdpTransportTests
    {
        private readonly ISerialization _mockJaegerThriftSerialization;
        private readonly ITProtocolFactory _protocolFactory;
        private readonly ISender _mockSender;
        
        private readonly int _jSpanSize;
        private readonly int _jProcessSize;
        //private readonly JaegerThriftTransport _testingTransport;

        public JaegerUdpTransportTests()
        {
            _mockJaegerThriftSerialization = Substitute.For<ISerialization>();
            _protocolFactory = new TCompactProtocol.Factory();
            _mockSender = Substitute.For<ISender>();
            _mockSender.ProtocolFactory.Returns(_protocolFactory);

            var jSpan = new JaegerSpan(10, 11, 12, 13, "opName", 0, 1234, 1235);
            _jSpanSize = CalcSizeOfSerializedThrift(jSpan);

            var jProcess = new JaegerProcess("testing");
            _jProcessSize = CalcSizeOfSerializedThrift(jProcess);

            _mockJaegerThriftSerialization.BuildJaegerProcessThrift(Arg.Any<IJaegerCoreTracer>()).Returns(jProcess);
            _mockJaegerThriftSerialization.BuildJaegerThriftSpan(Arg.Any<IJaegerCoreSpan>()).Returns(jSpan);
        }

        private static int CalcSizeOfSerializedThrift(TBase thriftBase)
        {
            var thriftBuffer = new TMemoryBuffer();
            var protocolFactory = new TCompactProtocol.Factory();
            var thriftProtocol = protocolFactory.GetProtocol(thriftBuffer);
            try
            {
                thriftBase.WriteAsync(thriftProtocol, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw new Exception("failed to calculate the size of a serialized thrift object", e);
            }
            return thriftBuffer.GetBuffer().Length;
        }

        private JaegerUdpTransport NewUdpTransportWithMaxPacketSize(int maxPacketSize)
        {
            return new JaegerUdpTransport(maxPacketSize, _mockSender, _mockJaegerThriftSerialization);
        }

        [Fact]
        public async void EmitBatchOverhead_ShouldNotGoOverOverheadConstant()
        {
            var transport = new TMemoryBuffer();
            var protocolFactory = new TCompactProtocol.Factory();
            var client = new Agent.Client(protocolFactory.GetProtocol(transport));

            var jSpan = new JaegerSpan(10, 11, 12, 13, "opName", 0, 1234, 1235);
            var jSpanSize = CalcSizeOfSerializedThrift(jSpan);

            var tests = new[] {1, 2, 14, 15, 377, 500, 65000, 0xFFFF};

            foreach (var test in tests)
            {
                transport.Reset();
                var batch = new List<JaegerSpan>();
                var processTags = new List<JaegerTag>();
                for (var j = 0; j < test; j++)
                {
                    batch.Add(jSpan);
                    processTags.Add(new JaegerTag("testingTag", TagType.BINARY) { VBinary = new byte[] { 0x20 }});
                }

                var jProcess = new JaegerProcess("testing") {Tags = processTags};
                await client.emitBatchAsync(new JaegerBatch(jProcess, batch), CancellationToken.None);
                var jProcessSize = CalcSizeOfSerializedThrift(jProcess);

                var overhead = transport.GetBuffer().Length - test * jSpanSize - jProcessSize;
                Assert.True(overhead <= TransportConstants.UdpEmitBatchOverhead);
            }
        }

        [Fact]
        public async void AppendAsync_ShouldThrowWhenSpanIsTooLarge()
        {
            var testingTransport = NewUdpTransportWithMaxPacketSize(_jSpanSize);

            var ex = await Assert.ThrowsAsync<Exception>(() => testingTransport.AppendAsync(Substitute.For<IJaegerCoreSpan>(), CancellationToken.None));

            Assert.Equal("Span too large to send over UDP", ex.Message);
        }


        [Fact]
        public async void AppendAsync_ShouldAddSpanToSenderAndFlushWhenItFitsJustRight()
        {
            var maxPacketSize = _jSpanSize + _jProcessSize + TransportConstants.UdpEmitBatchOverhead;
            var testingTransport = NewUdpTransportWithMaxPacketSize(maxPacketSize);
            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Returns(1);

            var sent = await testingTransport.AppendAsync(Substitute.For<IJaegerCoreSpan>(), CancellationToken.None);

            Assert.Equal(1, sent);
            _mockSender.Received(1).BufferItem(Arg.Any<JaegerSpan>());
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void AppendAsync_ShouldAddSpanToSenderAndReturnWhenThereIsMoreRoom()
        {
            var maxPacketSize = _jSpanSize + _jProcessSize + TransportConstants.UdpEmitBatchOverhead + 1;
            var testingTransport = NewUdpTransportWithMaxPacketSize(maxPacketSize);

            var sent = await testingTransport.AppendAsync(Substitute.For<IJaegerCoreSpan>(), CancellationToken.None);

            Assert.Equal(0, sent);
            _mockSender.Received(1).BufferItem(Arg.Any<JaegerSpan>());
            await _mockSender.Received(0).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void AppendAsync_ShouldFlushAndThenAddSpanWhenItDoesNotFit()
        {
            var maxPacketSize = _jSpanSize + _jProcessSize + TransportConstants.UdpEmitBatchOverhead + 1;
            var testingTransport = NewUdpTransportWithMaxPacketSize(maxPacketSize);

            _mockSender.FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>()).Returns(1);

            // send the first time - we should just buffer the item
            var sent = await testingTransport.AppendAsync(Substitute.For<IJaegerCoreSpan>(), CancellationToken.None);

            Assert.Equal(0, sent);
            _mockSender.Received(1).BufferItem(Arg.Any<JaegerSpan>());
            await _mockSender.Received(0).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());

            // send the second time - we should flush and then buffer again
            sent = await testingTransport.AppendAsync(Substitute.For<IJaegerCoreSpan>(), CancellationToken.None);

            Assert.Equal(1, sent);
            _mockSender.Received(2).BufferItem(Arg.Any<JaegerSpan>());
            await _mockSender.Received(1).FlushAsync(Arg.Any<JaegerProcess>(), Arg.Any<CancellationToken>());
        }
    }
}

