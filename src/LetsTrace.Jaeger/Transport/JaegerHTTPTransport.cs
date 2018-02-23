using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Thrift.Transports.Client;
using Thrift.Protocols;
using System.Threading;

using JaegerSpan = Jaeger.Thrift.Span;
using JaegerBatch = Jaeger.Thrift.Batch;

namespace LetsTrace.Jaeger.Transport
{
    public abstract class JaegerHTTPTransport : JaegerThriftTransport
    {
        public Uri Uri { get; }

        public JaegerHTTPTransport(Uri uri, int bufferSize) : base(bufferSize)
        {
            Uri = uri;
        }

        public override async Task Send(List<JaegerSpan> spans)
        {
            var batch = new JaegerBatch(_process, spans);
            var body = await SerializeThrift(batch);
            var content = new ByteArrayContent(body);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-thrift");
            
            var response = await HttpClient.PostAsync(Uri, content);

            response.EnsureSuccessStatusCode();
        }

        internal static async Task<byte[]> SerializeThrift(JaegerBatch batch)
        {
            var transport = new TMemoryBufferClientTransport();
            var protocol = new TBinaryProtocol(transport);

            await batch.WriteAsync(protocol, new CancellationToken());
            return transport.GetBuffer();
        }
    }
}