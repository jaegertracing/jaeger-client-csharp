using LetsTraceOperationSamplingStrategy = LetsTrace.Samplers.HTTP.OperationSamplingStrategy;
using JaegerThriftOperationSamplingStrategy = Jaeger.Thrift.Agent.OperationSamplingStrategy;

namespace LetsTrace.Jaeger.Samplers.Http
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