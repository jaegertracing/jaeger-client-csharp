using LetsTraceSamplingStrategyResponse = LetsTrace.Samplers.HTTP.SamplingStrategyResponse;
using JaegerThriftSamplingStrategyResponse = Jaeger.Thrift.Agent.SamplingStrategyResponse;

namespace LetsTrace.Jaeger.Samplers.Http
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