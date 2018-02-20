using System;
using System.Collections.Generic;

namespace LetsTrace.Samplers
{
    // NewProbabilisticSampler creates a sampler that randomly samples a certain percentage of traces specified by the
    // samplingRate, in the range between 0.0 and 1.0.
    public class ProbabilisticSampler : ISampler
    {
        private UInt64 _samplingBoundary;
        private Dictionary<string, Field> _tags;

        public double SamplingRate { get; }

        public ProbabilisticSampler(double samplingRate)
        {
            if (samplingRate < 0.0 || samplingRate > 1.0) throw new ArgumentOutOfRangeException(nameof(samplingRate), samplingRate, "sampling rate must be between 0.0 and 1.0");
            _tags = new Dictionary<string, Field> {
                { Constants.SamplerTypeTagKey, new Field<string> { Value = Constants.SamplerTypeConst } },
                { Constants.SamplerParamTagKey, new Field<double> { Value = samplingRate } }
            };

            _samplingBoundary = (UInt64) (UInt64.MaxValue * samplingRate);
            SamplingRate = samplingRate;
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
