using Jaeger.Thrift.Agent;
using LetsTrace.Jaeger.Transport.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocols;
using JaegerBatch = Jaeger.Thrift.Batch;
using JaegerSpan = Jaeger.Thrift.Span;

namespace LetsTrace.Jaeger.Transport
{
    public /*abstract*/ class JaegerUDPTransport : JaegerThriftTransport
    {
        // TODO: Constants
        public const string DEFAULT_AGENT_UDP_HOST = "localhost";
        public const int DEFAULT_AGENT_UDP_COMPACT_PORT = 6831;

        private readonly Agent.Client agentClient;
        private readonly ThriftUdpClientTransport udpTransport;

        /// <summary>
        /// This constructor expects Jaeger running running on <value>DEFAULT_AGENT_UDP_HOST</value>
        /// and port <value>DEFAULT_AGENT_UDP_COMPACT_PORT</value>
        /// </summary>
        public JaegerUDPTransport(int bufferSize = 0) : this(DEFAULT_AGENT_UDP_HOST, DEFAULT_AGENT_UDP_COMPACT_PORT, bufferSize)
        {
        }

        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="bufferSize">Buffer size</param>
        public JaegerUDPTransport(String host, int port, int bufferSize = 0) : base(new TCompactProtocol.Factory(), bufferSize)
        {
            if (string.IsNullOrEmpty(host))
            {
                host = DEFAULT_AGENT_UDP_HOST;
            }

            if (port == 0)
            {
                port = DEFAULT_AGENT_UDP_COMPACT_PORT;
            }

            udpTransport = new ThriftUdpClientTransport(host, port);
            agentClient = new Agent.Client(_protocolFactory.GetProtocol(udpTransport));
        }

        protected override Task SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken)
        {
            var batch = new JaegerBatch(_process, spans);
            return agentClient.emitBatchAsync(batch, cancellationToken);
        }

        public override async Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await base.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                udpTransport.Close();
            }
        }
    }
}