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

        /// <summary>
        /// Updates the probabilistic and lowerBound samplers.
        /// </summary>
        /// <param name="samplingRate">The sampling rate for probabilistic sampling</param>
        /// <param name="lowerBound">The lower bound limit for lower bound sampling</param>
        /// <returns>true, iff any samplers were updated</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Update(double samplingRate, double lowerBound)
        {
            bool isUpdated = false;
            if (Math.Abs(samplingRate - _probabilisticSampler.SamplingRate) > double.Epsilon)
            {
                _probabilisticSampler = new ProbabilisticSampler(samplingRate);
                ((Field<double>)_tags[Constants.SAMPLER_PARAM_TAG_KEY]).Value = samplingRate;
                isUpdated = true;
            }
            if (Math.Abs(lowerBound - _rateLimitingSampler.MaxTracesPerSecond) > double.Epsilon)
            {
                _rateLimitingSampler = new RateLimitingSampler(lowerBound);
                isUpdated = true;
            }
            return isUpdated;
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
