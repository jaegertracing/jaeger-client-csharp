using System.Collections.Generic;

namespace LetsTrace.Samplers.HTTP
{
    public class PerOperationSamplingStrategies
    {
        public double DefaultSamplingProbability { get; set; }

        public double DefaultLowerBoundTracesPerSecond { get; set; }

        public List<OperationSamplingStrategy> PerOperationStrategies { get; set; }
    }
}