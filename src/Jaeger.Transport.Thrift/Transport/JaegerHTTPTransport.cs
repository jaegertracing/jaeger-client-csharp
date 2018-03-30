using System;
using Jaeger.Transport.Thrift.Transport.Sender;
using Thrift.Protocols;

namespace Jaeger.Transport.Thrift.Transport
{
    public class JaegerHttpTransport : JaegerThriftTransport
    {
        public Uri Uri { get; }

        public JaegerHttpTransport(string hostname = TransportConstants.DefaultAgentHost, int port = TransportConstants.DefaultCollectorHttpJaegerThriftPort, int bufferSize = 0) 
            : this(new Uri($"http://{hostname}:{port}/api/traces"), bufferSize)
        {
        }

        protected JaegerHttpTransport(Uri uri, int bufferSize = 0) : base(new TBinaryProtocol.Factory(), new JaegerThriftHttpSender(uri), null, bufferSize)
        {
            Uri = uri;
        }
    }
}