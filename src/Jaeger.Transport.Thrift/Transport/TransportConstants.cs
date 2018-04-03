namespace Jaeger.Transport.Thrift.Transport
{
    public class TransportConstants
    {
        public const string DefaultAgentHost = "localhost";

        public const int DefaultAgentUdpZipkinCompactThriftPort = 5775;

        public const int DefaultAgentUdpJaegerCompactThriftPort = 6831;

        public const int DefaultAgentUdpJaegerBinaryThriftPort = 6832;

        public const int DefaultAgentHttpStrategiesPort = 5778;

        public const int DefaultCollectorHttpJaegerThriftPort = 14268;

        public const string CollectorHttpJaegerThriftFormatParam = "format=jaeger.thrift";

        // UdpPacketMaxLength is the max size of UDP packet we want to send, this is also the max size that jaeger-agent can handle
        public const int UdpPacketMaxLength = 65000;

        // Empirically obtained constant for how many bytes in the message are used for envelope.
        // The total datagram size is:
        // sizeof(Span) * numSpans + processByteSize + emitBatchOverhead <= maxPacketSize
        // There is a unit test `EmitBatchOverhead_ShouldNotGoOverOverheadConstant` that validates this number.
        // Note that due to the use of Compact Thrift protocol, overhead grows with the number of spans
        // in the batch, because the length of the list is encoded as varint32, as well as SeqId.
        public const int UdpEmitBatchOverhead = 22;
    }
}
