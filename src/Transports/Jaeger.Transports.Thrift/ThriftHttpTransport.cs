using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Jaeger.Senders.Thrift.Senders.Internal;
using Thrift.Protocol;
using Thrift.Transport;

namespace Jaeger.Transports.Thrift
{
    public class ThriftHttpTransport : ThriftTransport
    {
        private const string HttpCollectorJaegerThriftFormatParam = "format=jaeger.thrift";

        public ThriftHttpTransport(TTransport transport)
            : base(new TCompactProtocol.Factory(), transport)
        {
        }

        public override string ToString()
        {
            return $"{nameof(ThriftHttpTransport)}({base.ToString()})";
        }

        public sealed class Builder
        {
            internal string Endpoint { get; }
            internal AuthenticationHeaderValue AuthenticationHeaderValue { get; private set; }

            public Builder(string endpoint)
            {
                Endpoint = endpoint;
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

            public ThriftHttpTransport Build()
            {
                var collectorUri = new UriBuilder(Endpoint)
                {
                    Query = HttpCollectorJaegerThriftFormatParam
                }.Uri;

                var customHeaders = new Dictionary<string, string>();
                if (AuthenticationHeaderValue != null)
                {
                    customHeaders.Add("Authorization", AuthenticationHeaderValue.ToString());
                }

                var customProperties = new Dictionary<string, object>
                {
                    // Note: This ensures that internal requests from the tracer are not instrumented
                    // by https://github.com/opentracing-contrib/csharp-netcore
                    { "ot-ignore", true }
                };

                var transport = new THttpTransport(collectorUri, customHeaders, customProperties: customProperties);
                return new ThriftHttpTransport(transport);
            }
        }
    }
}