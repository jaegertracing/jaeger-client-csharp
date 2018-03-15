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
                { SamplerConstants.SamplerTypeTagKey, new Field<string> { Value = SamplerConstants.SamplerTypeConst } },
                { SamplerConstants.SamplerParamTagKey, new Field<bool> { Value = sample } }
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
