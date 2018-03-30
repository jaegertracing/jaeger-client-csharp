using System.Linq;
using JaegerPerOperationSamplingStrategies = Jaeger.Core.Samplers.HTTP.PerOperationSamplingStrategies;
using JaegerThriftPerOperationSamplingStrategies = Jaeger.Thrift.Agent.PerOperationSamplingStrategies;


namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class PerOperationSamplingStrategiesMarshaller
    {
        public static JaegerPerOperationSamplingStrategies FromThrift(this JaegerThriftPerOperationSamplingStrategies thriftInstance)
        {
            return new JaegerPerOperationSamplingStrategies
            {
                DefaultSamplingProbability = thriftInstance.DefaultSamplingProbability,
                DefaultLowerBoundTracesPerSecond = thriftInstance.DefaultLowerBoundTracesPerSecond,
                PerOperationStrategies = thriftInstance.PerOperationStrategies.Select(i => i.FromThrift()).ToList()
            };
        }
    }
}