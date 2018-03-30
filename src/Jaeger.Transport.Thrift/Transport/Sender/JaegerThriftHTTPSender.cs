using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocols;
using Thrift.Transports.Client;
using JaegerBatch = Jaeger.Thrift.Batch;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Transport.Sender
{
    public class JaegerThriftHttpSender : JaegerSender
    {
        private readonly TProtocol _protocol;

        internal JaegerThriftHttpSender(Uri uri) : base(new TBinaryProtocol.Factory())
        {
            var collectorUri = new UriBuilder(uri)
            {
                Query = TransportConstants.CollectorHttpJaegerThriftFormatParam
            }.Uri;
            var httpTransport = new THttpClientTransport(collectorUri, null);
            _protocol = _protocolFactory.GetProtocol(httpTransport);
        }

        protected override async Task<int> SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken)
        {
            var batch = new JaegerBatch(_process, spans);
            await batch.WriteAsync(_protocol, cancellationToken);
            await _protocol.Transport.FlushAsync(cancellationToken);

            return spans.Count;
        }
    }
}
