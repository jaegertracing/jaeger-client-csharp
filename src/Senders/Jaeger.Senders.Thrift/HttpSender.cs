using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Exceptions;
using Jaeger.Thrift.Senders.Internal;
using Thrift.Protocol;
using ThriftBatch = Jaeger.Thrift.Batch;
using ThriftProcess = Jaeger.Thrift.Process;
using ThriftSpan = Jaeger.Thrift.Span;

namespace Jaeger.Senders.Thrift
{
    public class HttpSender : ThriftSender
    {
        private const string HttpCollectorJaegerThriftFormatParam = "format=jaeger.thrift";
        private const int OneMbInBytes = 1048576;

        private readonly TProtocol _protocol;
        private readonly THttpTransport _transport;

        /// <summary>
        /// </summary>
        /// <param name="endpoint">Jaeger REST endpoint consuming jaeger.thrift, e.g
        /// http://localhost:14268/api/traces</param>
        public HttpSender(string endpoint)
            : this(new Builder(endpoint))
        {
        }

        private HttpSender(Builder builder)
            : base(ProtocolType.Binary, builder.MaxPacketSize)
        {
            Uri collectorUri = new UriBuilder(builder.Endpoint)
            {
                Query = HttpCollectorJaegerThriftFormatParam
            }.Uri;

            var customHeaders = new Dictionary<string, string>();
            if (builder.AuthenticationHeaderValue != null)
            {
                customHeaders.Add("Authorization", builder.AuthenticationHeaderValue.ToString());
            }

            var customProperties = new Dictionary<string, object>
            {
                // Note: This ensures that internal requests from the tracer are not instrumented
                // by https://github.com/opentracing-contrib/csharp-netcore
                { "ot-ignore", true }
            };

            _transport = new THttpTransport(collectorUri, customHeaders, builder.HttpHandler, builder.Certificates, builder.UserAgent, customProperties);
            _protocol = ProtocolFactory.GetProtocol(_transport);
        }

        protected override async Task SendAsync(ThriftProcess process, List<ThriftSpan> spans, CancellationToken cancellationToken)
        {
            try
            {
                var batch = new ThriftBatch(process, spans);
                await batch.WriteAsync(_protocol, cancellationToken).ConfigureAwait(false);
                await _protocol.Transport.FlushAsync(cancellationToken).ConfigureAwait(false);
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
                _transport.Close();
            }
        }

        public override string ToString()
        {
            return $"{nameof(HttpSender)}";
        }

        public sealed class Builder
        {
            internal string Endpoint { get; }
            internal int MaxPacketSize { get; private set; } = OneMbInBytes;
            internal AuthenticationHeaderValue AuthenticationHeaderValue { get; private set; }
            public HttpClientHandler HttpHandler { get; private set; }
            public IEnumerable<X509Certificate> Certificates { get; private set; }
            public string UserAgent { get; private set; }

            public Builder(string endpoint)
            {
                Endpoint = endpoint;
            }

            public Builder WithMaxPacketSize(int maxPacketSizeBytes)
            {
                MaxPacketSize = maxPacketSizeBytes;
                return this;
            }

            public Builder WithAuth(string username, string password)
            {
                string value = $"{username}:{password}";
                string encodedValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                AuthenticationHeaderValue = new AuthenticationHeaderValue("Basic", encodedValue);
                return this;
            }

            public Builder WithAuth(string authToken)
            {
                AuthenticationHeaderValue = new AuthenticationHeaderValue("Bearer", authToken);
                return this;
            }

            public Builder WithHttpHandler(HttpClientHandler httpHandler)
            {
                HttpHandler = httpHandler;
                return this;
            }

            public Builder WithCertificates(IEnumerable<X509Certificate> certificates)
            {
                Certificates = certificates;
                return this;
            }

            public Builder WithUserAgent(string userAgent)
            {
                UserAgent = userAgent;
                return this;
            }

            public HttpSender Build()
            {
                return new HttpSender(this);
            }
        }
    }
}
