using System;
using System.Collections.Generic;

namespace LetsTrace.Samplers
{
    /// <summary>
    /// Only used for unit testing
    /// </summary>
    internal interface IProbabilisticSampler : ISampler
    {
        double SamplingRate { get; }
    }

    // ProbabilisticSampler creates a sampler that randomly samples a certain percentage of traces specified by the
    // samplingRate, in the range between 0.0 and 1.0.
    public class ProbabilisticSampler : IProbabilisticSampler
    {
        // TODO: Constants!

        private readonly ulong _samplingBoundary;
        private readonly Dictionary<string, Field> _tags;

        public double SamplingRate { get; }

        public ProbabilisticSampler(double samplingRate = SamplerConstants.DefaultSamplingProbability)
        {
            if (samplingRate < 0.0 || samplingRate > 1.0) throw new ArgumentOutOfRangeException(nameof(samplingRate), samplingRate, "sampling rate must be between 0.0 and 1.0");
            SamplingRate = samplingRate;

            _samplingBoundary = (ulong) (ulong.MaxValue * samplingRate);
            _tags = new Dictionary<string, Field> {
                { SamplerConstants.SamplerTypeTagKey, new Field<string> { Value = SamplerConstants.SamplerTypeProbabilistic } },
                { SamplerConstants.SamplerParamTagKey, new Field<double> { Value = samplingRate } }
            };
        }

        public void Dispose()
        {
            // nothing to do
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
        {
            return (_samplingBoundary >= id.Low , _tags);
        }
    }
}
