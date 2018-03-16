using System;
using Thrift.Protocols;
using LetsTrace.Jaeger.Transport.Sender;

namespace LetsTrace.Jaeger.Transport
{
    public class JaegerHTTPTransport : JaegerThriftTransport
    {
        public Uri Uri { get; }

        public JaegerHTTPTransport(string hostname = TransportConstants.DefaultAgentHost, int port = TransportConstants.DefaultCollectorHTTPJaegerThriftPort, int bufferSize = 0) 
            : this(new Uri($"http://{hostname}:{port}/api/traces"), bufferSize)
        {
        }

        protected JaegerHTTPTransport(Uri uri, int bufferSize = 0) : base(new TBinaryProtocol.Factory(), new JaegerThriftHTTPSender(uri), null, bufferSize)
        {
            Uri = uri;
        }
    }
}