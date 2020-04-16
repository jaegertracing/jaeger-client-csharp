using Jaeger.Senders.Thrift.Senders.Internal;
using Microsoft.Extensions.Options;
using Thrift.Protocol;
using Thrift.Transport;

namespace Jaeger.Transports.Thrift
{
    public class ThriftUdpTransport : ThriftTransport
    {
        public const string DefaultAgentUdpHost = "localhost";
        public const int DefaultAgentUdpCompactPort = 6831;

        public ThriftUdpTransport(TTransport transport)
            : base(new TCompactProtocol.Factory(), transport)
        {
        }

        public override string ToString()
        {
            return $"{nameof(ThriftUdpTransport)}({base.ToString()})";
        }

        public sealed class Builder
        {
            internal string Host { get; private set; }
            internal int Port { get; private set; }

            public Builder()
            {
            }

            /// <param name="host">If empty it will use <see cref="DefaultAgentUdpHost"/>.</param>
            /// <param name="port">If 0 it will use <see cref="DefaultAgentUdpCompactPort"/>.</param>
            public Builder(string host, int port)
            {
                Host = host;
                Port = port;
            }

            public Builder WithHost(string host)
            {
                Host = host;
                return this;
            }

            public Builder WithPort(int port)
            {
                Port = port;
                return this;
            }

            public ThriftUdpTransport Build()
            {
                var host = Host;
                if (string.IsNullOrEmpty(host))
                {
                    host = DefaultAgentUdpHost;
                }

                var port = Port;
                if (port == 0)
                {
                    port = DefaultAgentUdpCompactPort;
                }

                var transport = new ThriftUdpClientTransport(host, port);
                return new ThriftUdpTransport(transport);
            }
        }
    }
}