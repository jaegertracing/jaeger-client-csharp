using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Thrift.Agent;
using Jaeger.Transport.Thrift.Transport.Internal;
using Thrift.Protocols;
using JaegerBatch = Jaeger.Thrift.Batch;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Transport.Sender
{
    public class JaegerThriftUdpSender : JaegerSender
    {
        private readonly Agent.Client _agentClient;

        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        public JaegerThriftUdpSender(string host, int port) : base(new TCompactProtocol.Factory())
        {
            if (string.IsNullOrEmpty(host))
            {
                host = TransportConstants.DefaultAgentHost;
            }

            if (port == 0)
            {
                port = TransportConstants.DefaultAgentUdpJaegerCompactThriftPort;
            }

            var udpThriftTransport = new ThriftUdpClientTransport(host, port);
            _agentClient = new Agent.Client(_protocolFactory.GetProtocol(udpThriftTransport));
        }

        protected override async Task<int> SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken)
        {
            var batch = new JaegerBatch(_process, spans);
            await _agentClient.emitBatchAsync(batch, cancellationToken);

            return spans.Count;
        }
    }
}
