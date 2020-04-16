using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Encoders;
using Jaeger.Thrift;
using Jaeger.Thrift.Agent;
using Jaeger.Transports.Thrift.Internal;
using Thrift.Protocol;
using Thrift.Transport;

namespace Jaeger.Transports.Thrift
{
    public abstract class ThriftTransport : ITransport
    {
        private readonly TTransport _transport;
        private readonly Agent.Client _agentClient;
        private readonly TMemoryBuffer _memoryTransport;

        public TProtocolFactory ProtocolFactory { get; }

        public ThriftTransport(TProtocolFactory protocolFactory, TTransport transport)
        {
            ProtocolFactory = protocolFactory;

            _transport = transport;
            _agentClient = new Agent.Client(protocolFactory.GetProtocol(transport));
            _memoryTransport = new TMemoryBuffer();
        }

        public int GetSize(IEncodedData data)
        {
            var encData = (TBase) data.Data;
            var protocol = ProtocolFactory.GetProtocol(_memoryTransport);

            _memoryTransport.Reset();
            encData.WriteAsync(protocol, CancellationToken.None).GetAwaiter().GetResult();
            return _memoryTransport.GetBuffer().Length;
        }

        public Task WriteBatchAsync(Batch batch, CancellationToken cancellationToken)
        {
            return _agentClient.emitBatchAsync(batch, cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(ThriftTransport)}(Transport={_transport})";
        }
    }
}