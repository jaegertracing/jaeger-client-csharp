using System;
using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers
{
    /// <summary>
    /// <see cref="ProbabilisticSampler"/> creates a sampler that randomly samples a certain percentage of traces specified by the
    /// samplingRate, in the range between 0.0 and 1.0.
    /// </summary>
    public class ProbabilisticSampler : ValueObject, ISampler
    {
        public const double DefaultSamplingProbability = 0.001;
        public const string Type = "probabilistic";

        private readonly long _positiveSamplingBoundary;
        private readonly long _negativeSamplingBoundary;
        private readonly IReadOnlyDictionary<string, object> _tags;

        public virtual double SamplingRate { get; }

        public ProbabilisticSampler(double samplingRate = DefaultSamplingProbability)
        {
            if (samplingRate < 0.0 || samplingRate > 1.0)
                throw new ArgumentOutOfRangeException(nameof(samplingRate), samplingRate, "sampling rate must be greater than 0.0 and less than 1.0");

            SamplingRate = samplingRate;

            // Note: Java uses bit shifting but that code results in a compiler warning in C#.
            // We could enclose it in an `unchecked` block but we're using this code
            // as it's more readable.
            _positiveSamplingBoundary = (long)(long.MaxValue * samplingRate);
            _negativeSamplingBoundary = (long)(long.MinValue * samplingRate);

            _tags = new Dictionary<string, object> {
                { Constants.SamplerTypeTagKey, Type },
                { Constants.SamplerParamTagKey, samplingRate }
            };
        }

        public virtual SamplingStatus Sample(string operation, TraceId id)
        {
            if (id.Low > 0)
            {
                return new SamplingStatus(id.Low <= _positiveSamplingBoundary, _tags);
            }
            else
            {
                return new SamplingStatus(id.Low >= _negativeSamplingBoundary, _tags);
            }
        }

        public override string ToString()
        {
            return $"{nameof(ProbabilisticSampler)}({SamplingRate})";
        }

        public void Close()
        {
            // nothing to do
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return SamplingRate;
        }
    }
}
