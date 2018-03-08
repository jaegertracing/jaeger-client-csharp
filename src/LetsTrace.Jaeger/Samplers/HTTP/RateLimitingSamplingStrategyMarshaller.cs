using LetsTrace.Samplers.HTTP;

namespace LetsTrace.Jaeger.Samplers.HTTP
{
    public static class RateLimitingSamplingStrategyMarshaller
    {
        public static RateLimitingSamplingStrategy FromThrift(this global::Jaeger.Thrift.Agent.RateLimitingSamplingStrategy thriftInstance)
        {
            return new RateLimitingSamplingStrategy
            {
                MaxTracesPerSecond = thriftInstance.MaxTracesPerSecond
            };
        }
    }
}