using Jaeger.Encoders.Thrift;
using Jaeger.Senders.SizedBatch;
using Jaeger.Transports.Thrift;

namespace Jaeger.Senders.Thrift
{
    /// <summary>
    /// This class is only for backwards compatibility and has no real function.
    /// </summary>
    public abstract class ThriftSender : SizedBatchSender
    {
        public ThriftSender(ThriftTransport transport, int maxPacketSize)
            : base(new ThriftEncoder(transport), maxPacketSize)
        {
        }
    }
}
