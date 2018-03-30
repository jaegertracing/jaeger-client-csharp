using JaegerSamplingStrategyResponse = Jaeger.Core.Samplers.HTTP.SamplingStrategyResponse;
using JaegerThriftSamplingStrategyResponse = Jaeger.Thrift.Agent.SamplingStrategyResponse;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class SamplingStrategyResponseMarshaller
    {
        public static JaegerSamplingStrategyResponse FromThrift(this JaegerThriftSamplingStrategyResponse thriftInstance)
        {
            return new JaegerSamplingStrategyResponse
            {
                ProbabilisticSampling = thriftInstance.ProbabilisticSampling.FromThrift(),
                RateLimitingSampling = thriftInstance.RateLimitingSampling.FromThrift(),
                OperationSampling = thriftInstance.OperationSampling.FromThrift()
            };
        }
    }
}