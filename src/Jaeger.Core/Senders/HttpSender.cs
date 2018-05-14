using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core.Exceptions;
using Jaeger.Thrift.Agent;
using Thrift.Transports.Client;
using ThriftBatch = Jaeger.Thrift.Batch;
using ThriftProcess = Jaeger.Thrift.Process;
using ThriftSpan = Jaeger.Thrift.Span;

namespace Jaeger.Core.Senders
{
    public class HttpSender : ThriftSender
    {
        private const string HttpCollectorJaegerThriftFormatParam = "format=jaeger.thrift";
        private const int OneMbInBytes = 1048576;

        private readonly Agent.Client _agentClient;
        private readonly THttpClientTransport _transport;

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
                customHeaders.Add("Authorize", builder.AuthenticationHeaderValue.ToString());
            }

            _transport = new THttpClientTransport(collectorUri, customHeaders);
            _agentClient = new Agent.Client(ProtocolFactory.GetProtocol(_transport));
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
                _transport.Close();
            }
        }

        public sealed class Builder
        {
            internal string Endpoint { get; }
            internal int MaxPacketSize { get; private set; } = OneMbInBytes;
            internal AuthenticationHeaderValue AuthenticationHeaderValue { get; private set; }

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

            public HttpSender Build()
            {
                return new HttpSender(this);
            }
        }
    }
}
