namespace LetsTrace.Propagation
{
    public interface IHeadersConfig
    {
        string TraceContextHeaderName { get; }
        string TraceBaggageHeaderPrefix { get; }
    }
}
