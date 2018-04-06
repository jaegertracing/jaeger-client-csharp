using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport.Internal;
using Jaeger.Transport.Thrift.Transport.Sender;
using Thrift.Protocols;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Transport
{
    /// <inheritdoc />
    /// <summary>
    /// JaegerUdpTransport provides an implementation to transport spans over UDP using
    /// Compact Thrift. It handles making sure payloads efficiently use the UDP packet
    /// size by filling up as much of a UDP message it can before sending.
    /// </summary>
    public class JaegerUdpTransport : JaegerThriftTransport
    {
        private readonly TMemoryBuffer _thriftBuffer;
        private readonly TProtocol _thriftProtocol;
        private int _byteBufferSize;
        private int _processByteSize;
        private readonly int _maxByteBufferSize;

        /// <inheritdoc />
        /// <summary>
        /// This constructor expects Jaeger running running on <value>TransportConstants.DefaultAgentHost</value>
        /// and port <value>TransportConstants.DefaultAgentUdpJaegerCompactThriftPort</value>
        /// </summary>
        public JaegerUdpTransport() : this(TransportConstants.DefaultAgentHost, TransportConstants.DefaultAgentUdpJaegerCompactThriftPort)
        {
        }

        /// <inheritdoc />
        /// <param name="host">The host the Jaeger Agent is running on</param>
        /// <param name="port">The port of the host that Jaeger Agent will accept compact thrift</param>
        /// <param name="maxPacketSize">The max byte size of UDP packets to be sent to the agent</param>
        public JaegerUdpTransport(string host, int port, int maxPacketSize = 0) : this(maxPacketSize, new JaegerThriftUdpSender(host, port), null)
        {
        }

        /// <inheritdoc />
        /// <param name="maxPacketSize">The max byte size of UDP packets to be sent to the agent</param>
        /// <param name="sender">The ISender that will take care of the actual sending as well as storing the items to be sent</param>
        /// <param name="serialization">The object that is used to serialize the spans into Thrift</param>
        internal JaegerUdpTransport(int maxPacketSize, ISender sender, ISerialization serialization = null)
            : base(sender, serialization)
        {
            // Each span is first written to thriftBuffer to determine its size in bytes.
            _thriftBuffer = new TMemoryBuffer();
            _thriftProtocol = _sender.ProtocolFactory.GetProtocol(_thriftBuffer);

            if (maxPacketSize == 0)
            {
                maxPacketSize = TransportConstants.UdpPacketMaxLength;
            }

            _maxByteBufferSize = maxPacketSize - TransportConstants.UdpEmitBatchOverhead;
        }

        /// <inheritdoc />
        /// <summary>
        /// ProtocolAppendLogicAsync contains the specific logic for appending spans to be
        /// sent over UDP. It will keep track of the byte size of the information to be sent
        /// to the Jaeger Agent and make sure that it does not go over the max UDP packet size
        /// by automatically flushing the buffer. It will throw and exception if the span is
        /// too large to send in one UDP packet by itself.
        /// </summary>
        /// <param name="span">The span serialized in Jaeger Thrift</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal override async Task<int> ProtocolAppendLogicAsync(JaegerSpan span, CancellationToken cancellationToken)
        {
            if (_processByteSize == 0) {
                _processByteSize = await CalcSizeOfSerializedThrift(_process);
                _byteBufferSize += _processByteSize;
            }

            var spanSize = await CalcSizeOfSerializedThrift(span);

            if (spanSize > _maxByteBufferSize) {
                throw new Exception("Span too large to send over UDP");
            }

            _byteBufferSize += spanSize;

            // if the span fits in the buffer, buffer it
            if (_byteBufferSize <= _maxByteBufferSize) {
                _sender.BufferItem(span);

                // if we can potentially fit more spans in the buffer don't flush
                if (_byteBufferSize < _maxByteBufferSize) {
                    return 0;
                }

                // can't fit anything else in the buffer, flush it
                _byteBufferSize = _processByteSize;
                return await FlushAsync(cancellationToken);
            }

            // the latest span did not fit in the buffer
            var sent = await FlushAsync(cancellationToken);

            _sender.BufferItem(span);
            _byteBufferSize = spanSize + _processByteSize;

            return sent;
        }

        /// <summary>
        /// CalcSizeOfSerializedThrift calculates the size in bytes that a Thrift object will take up.
        /// </summary>
        /// <param name="thriftBase">the base Thrift object to calculate the size of</param>
        /// <returns></returns>
        private async Task<int> CalcSizeOfSerializedThrift(TBase thriftBase)
        {
            _thriftBuffer.Reset();
            try {
                await thriftBase.WriteAsync(_thriftProtocol, CancellationToken.None);
            } catch (Exception e) {
                throw new Exception("failed to calculate the size of a serialized thrift object", e);
            }
            return _thriftBuffer.GetBuffer().Length;
        }
    }
}