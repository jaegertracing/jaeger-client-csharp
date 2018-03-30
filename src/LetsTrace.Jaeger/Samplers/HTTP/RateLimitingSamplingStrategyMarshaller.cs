using LetsTraceRateLimitingSamplingStrategy = Jaeger.Core.Samplers.HTTP.RateLimitingSamplingStrategy;
using JaegerThriftRateLimitingSamplingStrategy = Jaeger.Thrift.Agent.RateLimitingSamplingStrategy;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
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