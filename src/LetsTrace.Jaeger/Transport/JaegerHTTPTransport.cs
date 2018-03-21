using System;
using Thrift.Protocols;
using LetsTrace.Jaeger.Transport.Sender;

namespace LetsTrace.Jaeger.Transport
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