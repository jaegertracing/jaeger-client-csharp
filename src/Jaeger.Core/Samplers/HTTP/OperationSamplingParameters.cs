using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers.HTTP
{
    public class OperationSamplingParameters : ValueObject
    {
        public double DefaultSamplingProbability { get; }

        public double DefaultLowerBoundTracesPerSecond { get; }

        public List<PerOperationSamplingParameters> PerOperationStrategies { get; }

        public OperationSamplingParameters(
            double defaultSamplingProbability,
            double defaultLowerBoundTracesPerSecond,
            List<PerOperationSamplingParameters> perOperationStrategies)
        {
            DefaultSamplingProbability = defaultSamplingProbability;
            DefaultLowerBoundTracesPerSecond = defaultLowerBoundTracesPerSecond;
            PerOperationStrategies = perOperationStrategies;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return DefaultSamplingProbability;
            yield return DefaultLowerBoundTracesPerSecond;
            yield return PerOperationStrategies;
        }
    }
}