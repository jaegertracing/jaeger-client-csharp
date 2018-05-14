using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers.HTTP
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