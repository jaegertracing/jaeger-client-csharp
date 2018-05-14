using System.Collections.Generic;
using Jaeger.Util;

namespace Jaeger.Samplers.HTTP
{
    public class ProbabilisticSamplingStrategy : ValueObject
    {
        public double SamplingRate { get; }

        public ProbabilisticSamplingStrategy(double samplingRate)
        {
            SamplingRate = samplingRate;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return SamplingRate;
        }
    }
}