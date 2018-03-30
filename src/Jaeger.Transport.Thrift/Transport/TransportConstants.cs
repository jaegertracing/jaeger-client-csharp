namespace Jaeger.Transport.Thrift.Transport
{
    public class TransportConstants
    {
        public const string DefaultAgentHost = "localhost";

        public const int DefaultAgentUdpZipkinCompactThriftPort = 5775;

        public const int DefaultAgentUdpJaegerCompactThriftPort = 6831;

        public const int DefaultAgentUdpJaegerBinaryThriftPort = 6832;

        public const int DefaultAgentHttpStrategiesPort = 5778;

        public const int DefaultCollectorHttpJaegerThriftPort = 14268;

        public const string CollectorHttpJaegerThriftFormatParam = "format=jaeger.thrift";
    }
}
