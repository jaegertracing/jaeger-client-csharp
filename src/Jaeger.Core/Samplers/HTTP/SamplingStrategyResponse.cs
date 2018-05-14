using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers.HTTP
{
    public class SamplingStrategyResponse : ValueObject
    {
        public ProbabilisticSamplingStrategy ProbabilisticSampling { get; }
        public RateLimitingSamplingStrategy RateLimitingSampling { get; }
        public OperationSamplingParameters OperationSampling { get; }

        public SamplingStrategyResponse(
            ProbabilisticSamplingStrategy probabilisticSampling,
            RateLimitingSamplingStrategy rateLimitingSampling,
            OperationSamplingParameters operationSampling)
        {
            ProbabilisticSampling = probabilisticSampling;
            RateLimitingSampling = rateLimitingSampling;
            OperationSampling = operationSampling;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ProbabilisticSampling;
            yield return RateLimitingSampling;
            yield return OperationSampling;
        }
    }
}