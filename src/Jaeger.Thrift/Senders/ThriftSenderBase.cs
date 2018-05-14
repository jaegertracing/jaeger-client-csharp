using System;
using System.Threading;
using Jaeger.Thrift.Senders.Internal;
using Thrift.Protocols;

namespace Jaeger.Thrift.Senders
{
    public abstract class ThriftSenderBase
    {
        public enum ProtocolType
        {
            Binary,
            Compact
        }

        public const int EmitBatchOverhead = 33;

        private readonly TMemoryBuffer _memoryTransport;

        protected ITProtocolFactory ProtocolFactory { get; }

        protected int MaxSpanBytes { get; }

        /// <param name="protocolType">Protocol type (compact or binary)</param<
        /// <param name="maxPacketSize">If 0 it will use default value <see cref="ThriftUdpTransport.MAX_PACKET_SIZE"/>.</param>
        public ThriftSenderBase(ProtocolType protocolType, int maxPacketSize)
        {
            switch (protocolType)
            {
                case ProtocolType.Binary:
                    ProtocolFactory = new TBinaryProtocol.Factory();
                    break;
                case ProtocolType.Compact:
                    ProtocolFactory = new TCompactProtocol.Factory();
                    break;
                default:
                    throw new NotSupportedException("Unknown thrift protocol type specified: " + protocolType);
            }

            if (maxPacketSize == 0)
            {
                maxPacketSize = ThriftUdpClientTransport.MaxPacketSize;
            }

            MaxSpanBytes = maxPacketSize - EmitBatchOverhead;
            _memoryTransport = new TMemoryBuffer();
        }

        public int GetSize(TBase thriftBase)
        {
            _memoryTransport.Reset();
            thriftBase.WriteAsync(ProtocolFactory.GetProtocol(_memoryTransport), CancellationToken.None).GetAwaiter().GetResult();
            return _memoryTransport.GetBuffer().Length;
        }
    }
}
