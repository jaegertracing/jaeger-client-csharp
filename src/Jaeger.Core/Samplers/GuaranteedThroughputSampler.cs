using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers
{
    /// <summary>
    /// <see cref="GuaranteedThroughputSampler"/> is a <see cref="ISampler"/> that guarantees a throughput by using
    /// a <see cref="ProbabilisticSampler"/> and <see cref="RateLimitingSampler"/> in tandem.
    /// <para/>
    /// The <see cref="RateLimitingSampler"/> is used to establish a lowerBound so that every operation is sampled
    /// at least once in the time interval defined by the lowerBound.
    /// </summary>
    public class GuaranteedThroughputSampler : ValueObject, ISampler
    {
        public const string Type = "lowerbound";

        private readonly object _lock = new object();
        private ProbabilisticSampler _probabilisticSampler;
        private RateLimitingSampler _lowerBoundSampler;
        private IReadOnlyDictionary<string, object> _tags;

        public GuaranteedThroughputSampler(double samplingRate, double lowerBound)
        {
            _tags = new Dictionary<string, object> {
                { Constants.SamplerTypeTagKey, Type },
                { Constants.SamplerParamTagKey, samplingRate }
            };

            _probabilisticSampler = new ProbabilisticSampler(samplingRate);
            _lowerBoundSampler = new RateLimitingSampler(lowerBound);
        }

        /// <summary>
        /// Updates the probabilistic and lowerBound samplers.
        /// </summary>
        /// <param name="samplingRate">The sampling rate for probabilistic sampling.</param>
        /// <param name="lowerBound">The lower bound limit for lower bound sampling.</param>
        /// <returns><c>true</c>, if any samplers were updated.</returns>
        public virtual bool Update(double samplingRate, double lowerBound)
        {
            lock (_lock)
            {
                var isUpdated = false;
                if (samplingRate != _probabilisticSampler.SamplingRate)
                {
                    _probabilisticSampler.Close();
                    _probabilisticSampler = new ProbabilisticSampler(samplingRate);

                    var newTags = new Dictionary<string, object>();
                    foreach (var oldTag in _tags)
                    {
                        newTags[oldTag.Key] = oldTag.Value;
                    }
                    newTags[Constants.SamplerParamTagKey] = samplingRate;

                    _tags = newTags;
                    isUpdated = true;
                }
                if (lowerBound != _lowerBoundSampler.MaxTracesPerSecond)
                {
                    _lowerBoundSampler.Close();
                    _lowerBoundSampler = new RateLimitingSampler(lowerBound);
                    isUpdated = true;
                }
                return isUpdated;
            }
        }

        /// <summary>
        /// Calls <see cref="ISampler.Sample(string, TraceId)"/> on both samplers, returning <c>true</c> for
        /// <see cref="SamplingStatus.IsSampled"/> if either samplers set <see cref="SamplingStatus.IsSampled"/> to <c>true</c>.
        /// The tags corresponding to the sampler that returned <c>true</c> are set on <see cref="SamplingStatus.Tags"/>.
        /// If both samplers return <c>true</c>, tags for <see cref="ProbabilisticSampler"/> is given priority.
        /// </summary>
        /// <param name="operation">The operation name, which is ignored by this sampler.</param>
        /// <param name="id">The traceId on the span.</param>
        public virtual SamplingStatus Sample(string operation, TraceId id)
        {
            lock (_lock)
            {
                var probabilisticSamplingStatus = _probabilisticSampler.Sample(operation, id);
                var lowerBoundSamplingStatus = _lowerBoundSampler.Sample(operation, id);

                if (probabilisticSamplingStatus.IsSampled)
                {
                    return probabilisticSamplingStatus;
                }

                return new SamplingStatus(lowerBoundSamplingStatus.IsSampled, _tags);
            }
        }

        public override string ToString()
        {
            lock (_lock)
            {
                return $"{nameof(GuaranteedThroughputSampler)}({_probabilisticSampler}/{_lowerBoundSampler})";
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                _probabilisticSampler.Close();
                _lowerBoundSampler.Close();
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _lowerBoundSampler;
            yield return _probabilisticSampler;
        }
    }
}
