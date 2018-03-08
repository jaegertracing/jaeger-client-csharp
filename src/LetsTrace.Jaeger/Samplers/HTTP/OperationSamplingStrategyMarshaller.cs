using LetsTrace.Samplers.HTTP;

namespace LetsTrace.Jaeger.Samplers.HTTP
{
    public static class OperationSamplingStrategyMarshaller
    {
        public static OperationSamplingStrategy FromThrift(this global::Jaeger.Thrift.Agent.OperationSamplingStrategy thriftInstance)
        {
            return new OperationSamplingStrategy
            {
                Operation = thriftInstance.Operation,
                ProbabilisticSampling = thriftInstance.ProbabilisticSampling.FromThrift()
            };
        }
    }
}