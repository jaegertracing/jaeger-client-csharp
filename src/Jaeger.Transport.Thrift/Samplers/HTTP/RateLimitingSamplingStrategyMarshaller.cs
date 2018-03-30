using JaegerRateLimitingSamplingStrategy = Jaeger.Core.Samplers.HTTP.RateLimitingSamplingStrategy;
using JaegerThriftRateLimitingSamplingStrategy = Jaeger.Thrift.Agent.RateLimitingSamplingStrategy;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class RateLimitingSamplingStrategyMarshaller
    {
        public static JaegerRateLimitingSamplingStrategy FromThrift(this JaegerThriftRateLimitingSamplingStrategy thriftInstance)
        {
            return new JaegerRateLimitingSamplingStrategy
            {
                MaxTracesPerSecond = thriftInstance.MaxTracesPerSecond
            };
        }
    }
}