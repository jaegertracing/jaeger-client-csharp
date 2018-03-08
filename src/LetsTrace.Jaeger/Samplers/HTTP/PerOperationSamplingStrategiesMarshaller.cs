using System.Linq;
using LetsTrace.Samplers.HTTP;

namespace LetsTrace.Jaeger.Samplers.HTTP
{
    public static class PerOperationSamplingStrategiesMarshaller
    {
        public static PerOperationSamplingStrategies FromThrift(this global::Jaeger.Thrift.Agent.PerOperationSamplingStrategies thriftInstance)
        {
            return new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = thriftInstance.DefaultSamplingProbability,
                DefaultLowerBoundTracesPerSecond = thriftInstance.DefaultLowerBoundTracesPerSecond,
                PerOperationStrategies = thriftInstance.PerOperationStrategies.Select(i => i.FromThrift()).ToList()
            };
        }
    }
}