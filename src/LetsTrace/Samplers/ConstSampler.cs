using System.Collections.Generic;

namespace LetsTrace.Samplers
{
    // ConstSampler is a sampler that always makes the same decision.
    public class ConstSampler : ISampler
    {
        public bool Decision { get; }

        private readonly Dictionary<string, Field> _tags;

        public ConstSampler(bool sample)
        {
            Decision = sample;
            _tags = new Dictionary<string, Field> {
                { SamplingConstants.SAMPLER_TYPE_TAG_KEY, new Field<string> { Value = SamplingConstants.SAMPLER_TYPE_CONST } },
                { SamplingConstants.SAMPLER_PARAM_TAG_KEY, new Field<bool> { Value = sample } }
            };
        }

        public void Dispose()
        {
            // nothing to do
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
        {
            return (Decision, _tags);
        }
    }
}
