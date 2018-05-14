using Jaeger.Core.Samplers;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class ConstSamplerTests
    {
        [Fact]
        public void TestTags()
        {
            ConstSampler sampler = new ConstSampler(true);
            var tags = sampler.Sample("biryani", new TraceId(1)).Tags;

            Assert.Equal("const", tags["sampler.type"]);
            Assert.True((bool)tags["sampler.param"]);
        }
    }
}
