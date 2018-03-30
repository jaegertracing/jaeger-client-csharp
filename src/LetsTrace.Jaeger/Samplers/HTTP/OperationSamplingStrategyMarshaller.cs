using LetsTraceOperationSamplingStrategy = Jaeger.Core.Samplers.HTTP.OperationSamplingStrategy;
using JaegerThriftOperationSamplingStrategy = Jaeger.Thrift.Agent.OperationSamplingStrategy;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class OperationSamplingStrategyMarshaller
    {
        public static LetsTraceOperationSamplingStrategy FromThrift(this JaegerThriftOperationSamplingStrategy thriftInstance)
        {
            return new LetsTraceOperationSamplingStrategy
            {
                Operation = thriftInstance.Operation,
                ProbabilisticSampling = thriftInstance.ProbabilisticSampling.FromThrift()
            };
        }
    }
}