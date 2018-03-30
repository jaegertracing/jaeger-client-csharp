using Jaeger.Core.Samplers.HTTP;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class ProbabilisticSamplingStrategyMarshaller
    {
        public static ProbabilisticSamplingStrategy FromThrift(this global::Jaeger.Thrift.Agent.ProbabilisticSamplingStrategy thriftInstance)
        {
            return new ProbabilisticSamplingStrategy
            {
                SamplingRate = thriftInstance.SamplingRate
            };
        }
    }
}