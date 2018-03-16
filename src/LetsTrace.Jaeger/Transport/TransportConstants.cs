using System;
using System.Collections.Generic;
using System.Text;

namespace LetsTrace.Jaeger.Transport
{
    public class TransportConstants
    {
        public const string DefaultAgentHost = "localhost";

        public const int DefaultAgentUDPZipkinCompactThriftPort = 5775;

        public const int DefaultAgentUDPJaegerCompactThriftPort = 6831;

        public const int DefaultAgentUDPJaegerBinaryThriftPort = 6832;

        public const int DefaultAgentHTTPStrategiesPort = 5778;

        public const int DefaultCollectorHTTPJaegerThriftPort = 14268;

        public const string CollectorHTTPJaegerThriftFormatParam = "format=jaeger.thrift";
    }
}
