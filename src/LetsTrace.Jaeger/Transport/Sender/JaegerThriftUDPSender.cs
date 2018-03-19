using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Thrift.Agent;
using LetsTrace.Jaeger.Transport.Internal;
using Thrift.Protocols;
using JaegerBatch = Jaeger.Thrift.Batch;
using JaegerSpan = Jaeger.Thrift.Span;

namespace LetsTrace.Jaeger.Transport.Sender
{
    public class JaegerThriftUDPSender : JaegerSender
    {
        private readonly Agent.Client _agentClient;
        private readonly ThriftUdpClientTransport _udpThriftTransport;

        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        public JaegerThriftUDPSender(string host, int port) : base(new TCompactProtocol.Factory())
        {
            if (string.IsNullOrEmpty(host))
            {
                host = TransportConstants.DefaultAgentHost;
            }

            if (port == 0)
            {
                port = TransportConstants.DefaultAgentUDPJaegerCompactThriftPort;
            }

            _udpThriftTransport = new ThriftUdpClientTransport(host, port);
            _agentClient = new Agent.Client(_protocolFactory.GetProtocol(_udpThriftTransport));
        }

        protected override async Task<int> SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken)
        {
            var batch = new JaegerBatch(_process, spans);
            await _agentClient.emitBatchAsync(batch, cancellationToken);

            return spans.Count;
        }
    }
}
