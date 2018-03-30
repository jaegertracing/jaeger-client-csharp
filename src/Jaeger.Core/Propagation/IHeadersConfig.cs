namespace Jaeger.Core.Propagation
{
    public interface IHeadersConfig
    {
        string TraceContextHeaderName { get; }
        string TraceBaggageHeaderPrefix { get; }
    }
}
