using JaegerOperationSamplingStrategy = Jaeger.Core.Samplers.HTTP.OperationSamplingStrategy;
using JaegerThriftOperationSamplingStrategy = Jaeger.Thrift.Agent.OperationSamplingStrategy;

namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class OperationSamplingStrategyMarshaller
    {
        public static JaegerOperationSamplingStrategy FromThrift(this JaegerThriftOperationSamplingStrategy thriftInstance)
        {
            return new JaegerOperationSamplingStrategy
            {
                Operation = thriftInstance.Operation,
                ProbabilisticSampling = thriftInstance.ProbabilisticSampling.FromThrift()
            };
        }
    }
}