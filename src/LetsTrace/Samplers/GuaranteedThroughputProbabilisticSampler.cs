using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LetsTrace.Samplers
{
    /// <summary>
    /// Only used for unit testing
    /// </summary>
    internal interface IGuaranteedThroughputProbabilisticSampler : ISampler
    {
        bool Update(double samplingRate, double lowerBound);
    }

    // GuaranteedThroughputProbabilisticSampler is a sampler that leverages both ProbabilisticSampler and
    // RateLimitingSampler. The RateLimitingSampler is used as a guaranteed lower bound sampler such that
    // every operation is sampled at least once in a time interval defined by the lowerBound. ie a lowerBound
    // of 1.0 / (60 * 10) will sample an operation at least once every 10 minutes.
    public class GuaranteedThroughputProbabilisticSampler : IGuaranteedThroughputProbabilisticSampler
    {
        private IProbabilisticSampler _probabilisticSampler;
        private IRateLimitingSampler _rateLimitingSampler;
        private Dictionary<string, Field> _tags;

        public GuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound)
            : this(new ProbabilisticSampler(samplingRate), new RateLimitingSampler(lowerBound))
        {}

        internal GuaranteedThroughputProbabilisticSampler(IProbabilisticSampler probabilisticSampler, IRateLimitingSampler rateLimitingSampler)
        {
            _probabilisticSampler = probabilisticSampler;
            _rateLimitingSampler = rateLimitingSampler;
            _tags = new Dictionary<string, Field> {
                { Constants.SAMPLER_TYPE_TAG_KEY, new Field<string> { Value = Constants.SAMPLER_TYPE_LOWERBOUND } },
                { Constants.SAMPLER_PARAM_TAG_KEY, new Field<double> { Value = _probabilisticSampler.SamplingRate } }
            };
        }

        public void Dispose()
        {
            _probabilisticSampler.Dispose();
            _rateLimitingSampler.Dispose();
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
        {
            var probabilisticSampling = _probabilisticSampler.IsSampled(id, operation);
            var rateLimitingSampling = _rateLimitingSampler.IsSampled(id, operation);

            if (probabilisticSampling.Sampled) {
                return probabilisticSampling;
            }

            return rateLimitingSampling;
        }
    }
}
