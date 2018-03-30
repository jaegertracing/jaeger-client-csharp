using LetsTraceSamplingStrategyResponse = Jaeger.Core.Samplers.HTTP.SamplingStrategyResponse;
using JaegerThriftSamplingStrategyResponse = Jaeger.Thrift.Agent.SamplingStrategyResponse;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class SamplingStrategyResponseMarshaller
    {
        public static LetsTraceSamplingStrategyResponse FromThrift(this JaegerThriftSamplingStrategyResponse thriftInstance)
        {
            return new LetsTraceSamplingStrategyResponse
            {
                ProbabilisticSampling = thriftInstance.ProbabilisticSampling.FromThrift(),
                RateLimitingSampling = thriftInstance.RateLimitingSampling.FromThrift(),
                OperationSampling = thriftInstance.OperationSampling.FromThrift()
            };
        }
    }
}