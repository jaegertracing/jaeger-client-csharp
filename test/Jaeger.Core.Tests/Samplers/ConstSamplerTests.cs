using System.Collections.Generic;
using Jaeger.Core.Samplers;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class ConstSamplerTests
    {
        [Fact]
        public void ConstSampler_Constructor()
        {
            var sample = true;
            var expectedTags = new Dictionary<string, object> {
                { SamplerConstants.SamplerTypeTagKey, SamplerConstants.SamplerTypeConst },
                { SamplerConstants.SamplerParamTagKey, sample }
            };
            var sampler = new ConstSampler(sample);

            var isSampled = sampler.IsSampled(new TraceId(1), "op");

            Assert.Equal(sample, sampler.Decision);
            Assert.Equal(sample, isSampled.Sampled);
            Assert.Equal(expectedTags, isSampled.Tags);
            sampler.Dispose();
        }
    }
}
