namespace Jaeger.Core.Propagation
{
    public class HeadersConfig : IHeadersConfig
    {
        public string TraceContextHeaderName { get; }
        public string TraceBaggageHeaderPrefix { get; }

        public HeadersConfig(string traceContextHeaderName, string traceBaggageHeaderPrefix)
        {
            TraceContextHeaderName = traceContextHeaderName;
            TraceBaggageHeaderPrefix = traceBaggageHeaderPrefix;
        }
    }
}
