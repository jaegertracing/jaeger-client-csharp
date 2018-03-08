using LetsTrace.Samplers.HTTP;

namespace LetsTrace.Jaeger.Samplers.HTTP
{
    public static class SamplingStrategyResponseMarshaller
    {
        public static SamplingStrategyResponse FromThrift(this global::Jaeger.Thrift.Agent.SamplingStrategyResponse thriftInstance)
        {
            return new SamplingStrategyResponse
            {
                ProbabilisticSampling = thriftInstance.ProbabilisticSampling.FromThrift(),
                RateLimitingSampling = thriftInstance.RateLimitingSampling.FromThrift(),
                OperationSampling = thriftInstance.OperationSampling.FromThrift()
            };
        }
    }
}