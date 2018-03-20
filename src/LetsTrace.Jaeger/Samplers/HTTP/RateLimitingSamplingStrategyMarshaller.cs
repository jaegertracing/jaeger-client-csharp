using LetsTraceRateLimitingSamplingStrategy = LetsTrace.Samplers.HTTP.RateLimitingSamplingStrategy;
using JaegerThriftRateLimitingSamplingStrategy = Jaeger.Thrift.Agent.RateLimitingSamplingStrategy;

namespace LetsTrace.Jaeger.Samplers.Http
{
    public static class RateLimitingSamplingStrategyMarshaller
    {
        public static LetsTraceRateLimitingSamplingStrategy FromThrift(this JaegerThriftRateLimitingSamplingStrategy thriftInstance)
        {
            return new LetsTraceRateLimitingSamplingStrategy
            {
                MaxTracesPerSecond = thriftInstance.MaxTracesPerSecond
            };
        }
    }
}