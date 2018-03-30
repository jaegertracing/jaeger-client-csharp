using System.Linq;
using LetsTracePerOperationSamplingStrategies = Jaeger.Core.Samplers.HTTP.PerOperationSamplingStrategies;
using JaegerThriftPerOperationSamplingStrategies = Jaeger.Thrift.Agent.PerOperationSamplingStrategies;


namespace Jaeger.Transport.Thrift.Samplers.HTTP
{
    public static class PerOperationSamplingStrategiesMarshaller
    {
        public static LetsTracePerOperationSamplingStrategies FromThrift(this JaegerThriftPerOperationSamplingStrategies thriftInstance)
        {
            return new LetsTracePerOperationSamplingStrategies
            {
                DefaultSamplingProbability = thriftInstance.DefaultSamplingProbability,
                DefaultLowerBoundTracesPerSecond = thriftInstance.DefaultLowerBoundTracesPerSecond,
                PerOperationStrategies = thriftInstance.PerOperationStrategies.Select(i => i.FromThrift()).ToList()
            };
        }
    }
}