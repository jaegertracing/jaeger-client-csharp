using System.Collections.Generic;
using Jaeger.Util;

namespace Jaeger.Samplers.HTTP
{
    public class RateLimitingSamplingStrategy : ValueObject
    {
        public double MaxTracesPerSecond { get; }

        public RateLimitingSamplingStrategy(double maxTracesPerSecond)
        {
            MaxTracesPerSecond = maxTracesPerSecond;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return MaxTracesPerSecond;
        }
    }
}