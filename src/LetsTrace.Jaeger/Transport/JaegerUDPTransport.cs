using LetsTrace.Jaeger.Transport.Sender;
using Thrift.Protocols;

namespace LetsTrace.Jaeger.Transport
{
    public class JaegerUdpTransport : JaegerThriftTransport
    {
        /// <inheritdoc />
        /// <summary>
        /// This constructor expects Jaeger running running on <value>DEFAULT_AGENT_UDP_HOST</value>
        /// and port <value>DEFAULT_AGENT_UDP_COMPACT_PORT</value>
        /// </summary>
        public JaegerUdpTransport(int bufferSize = 0) : this(TransportConstants.DefaultAgentHost, TransportConstants.DefaultAgentUdpJaegerCompactThriftPort, bufferSize)
        {
        }

        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="bufferSize">Buffer size</param>
        public JaegerUdpTransport(string host, int port, int bufferSize = 0) : base(new TCompactProtocol.Factory(), new JaegerThriftUdpSender(host, port), null, bufferSize)
        {
        }
    }
}