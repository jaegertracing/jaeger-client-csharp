using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core.Exceptions;
using Jaeger.Thrift.Agent;
using Jaeger.Thrift.Senders.Internal;
using ThriftBatch = Jaeger.Thrift.Batch;
using ThriftProcess = Jaeger.Thrift.Process;
using ThriftSpan = Jaeger.Thrift.Span;

namespace Jaeger.Core.Senders
{
    /// <inheritdoc />
    /// <summary>
    /// JaegerUdpTransport provides an implementation to transport spans over UDP using
    /// Compact Thrift. It handles making sure payloads efficiently use the UDP packet
    /// size by filling up as much of a UDP message it can before sending.
    /// </summary>
    public class UdpSender : ThriftSender
    {
        public const string DefaultAgentUdpHost = "localhost";
        public const int DefaultAgentUdpCompactPort = 6831;

        private readonly Agent.Client _agentClient;
        private readonly ThriftUdpClientTransport _udpTransport;

        /// <summary>
        /// This constructor expects Jaeger running on <see cref="DefaultAgentUdpHost"/>
        /// and port <see cref="DefaultAgentUdpCompactPort"/>.
        /// </summary>
        public UdpSender()
            : this(DefaultAgentUdpHost, DefaultAgentUdpCompactPort, 0)
        {
        }

        /// <param name="host">If empty it will use <see cref="DefaultAgentUdpHost"/>.</param>
        /// <param name="port">If 0 it will use <see cref="DefaultAgentUdpCompactPort"/>.</param>
        /// <param name="maxPacketSize">If 0 it will use <see cref="ThriftUdpClientTransport.MaxPacketSize"/>.</param>
        public UdpSender(string host, int port, int maxPacketSize)
            : base(ProtocolType.Compact, maxPacketSize)
        {

            if (string.IsNullOrEmpty(host))
            {
                host = DefaultAgentUdpHost;
            }

            if (port == 0)
            {
                port = DefaultAgentUdpCompactPort;
            }

            _udpTransport = new ThriftUdpClientTransport(host, port);
            _agentClient = new Agent.Client(ProtocolFactory.GetProtocol(_udpTransport));
        }

        protected override async Task SendAsync(ThriftProcess process, List<ThriftSpan> spans, CancellationToken cancellationToken)
        {
            try
            {
                var batch = new ThriftBatch(process, spans);
                await _agentClient.emitBatchAsync(batch, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SenderException($"Could not send {spans.Count} spans", ex, spans.Count);
            }
        }

        public override async Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await base.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _udpTransport.Close();
            }
        }
    }
}