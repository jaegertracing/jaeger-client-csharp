using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thrift.Transports.Client;
using Thrift.Protocols;
using System.Threading;

using JaegerSpan = Jaeger.Thrift.Span;
using JaegerBatch = Jaeger.Thrift.Batch;

namespace LetsTrace.Jaeger.Transport
{
    public /*abstract*/ class JaegerHTTPTransport : JaegerThriftTransport
    {
        public Uri Uri { get; }

        // TODO: Constants
        public const string DEFAULT_AGENT_HTTP_HOST = "localhost";
        public const int DEFAULT_AGENT_HTTP_BINARY_PORT = 14268;
        private const string HTTP_COLLECTOR_JAEGER_THRIFT_FORMAT_PARAM = "format=jaeger.thrift"; // TODO: Constants

        private readonly THttpClientTransport httpTransport;
        private readonly TProtocol protocol;

        public JaegerHTTPTransport(string hostname = DEFAULT_AGENT_HTTP_HOST, int port = DEFAULT_AGENT_HTTP_BINARY_PORT) 
            : this(new Uri($"http://{hostname}:{port}/api/traces"))
        {
        }

        protected JaegerHTTPTransport(Uri uri, int bufferSize = 0) : base(new TBinaryProtocol.Factory(), bufferSize)
        {
            Uri = uri;
            Uri collectorUri = new UriBuilder(uri)
            {
                Query = HTTP_COLLECTOR_JAEGER_THRIFT_FORMAT_PARAM
            }.Uri;
            httpTransport = new THttpClientTransport(collectorUri, null);
            protocol = _protocolFactory.GetProtocol(httpTransport);
        }

        protected override async Task SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken)
        {
            var batch = new JaegerBatch(_process, spans);
            await batch.WriteAsync(protocol, cancellationToken);
            await protocol.Transport.FlushAsync(cancellationToken);
        }

        public override async Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await base.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                httpTransport.Close();
            }
        }
    }
}