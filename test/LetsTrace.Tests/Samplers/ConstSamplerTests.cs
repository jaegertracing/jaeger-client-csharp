using System.Collections.Generic;
using LetsTrace.Samplers;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class ConstSamplerTests
    {
        [Fact]
        public void ConstSampler_Constructor()
        {
            var sample = true;
            var expectedTags = new Dictionary<string, Field> {
                { Constants.SamplerTypeTagKey, new Field<string> { Value = Constants.SamplerTypeConst } },
                { Constants.SamplerParamTagKey, new Field<bool> { Value = sample } }
            };
            var sampler = new ConstSampler(sample);

            var isSampled = sampler.IsSampled(new TraceId(), "op");

            Assert.Equal(sample, sampler.Decision);
            Assert.Equal(sample, isSampled.Sampled);
            Assert.Equal(expectedTags, isSampled.Tags);
        }
    }
}
