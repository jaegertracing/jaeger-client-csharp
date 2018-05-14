using System;
using Jaeger.Core.Samplers;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class ProbabilisticSamplerTests
    {
        [Fact]
        public void TestSamplingBoundariesPositive()
        {
            double samplingRate = 0.5;

            long halfwayBoundary = 0x3fffffffffffffffL;
            ISampler sampler = new ProbabilisticSampler(samplingRate);
            Assert.True(sampler.Sample("", new TraceId(halfwayBoundary)).IsSampled);

            Assert.False(sampler.Sample("", new TraceId(halfwayBoundary + 2)).IsSampled);
        }

        [Fact]
        public void TestSamplingBoundariesNegative()
        {
            double samplingRate = 0.5;

            long halfwayBoundary = -0x4000000000000000L;
            ISampler sampler = new ProbabilisticSampler(samplingRate);
            Assert.True(sampler.Sample("", new TraceId(halfwayBoundary)).IsSampled);

            Assert.False(sampler.Sample("", new TraceId(halfwayBoundary - 1)).IsSampled);
        }

        [Fact]
        public void TestSamplerThrowsInvalidSamplingRangeExceptionUnder()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProbabilisticSampler(-0.1));
        }

        [Fact]
        public void TestSamplerThrowsInvalidSamplingRangeExceptionOver()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProbabilisticSampler(1.1));
        }

        [Fact]
        public void TestTags()
        {
            ProbabilisticSampler sampler = new ProbabilisticSampler(0.1);
            var tags = sampler.Sample("vadacurry", new TraceId(20L)).Tags;
            Assert.Equal("probabilistic", tags["sampler.type"]);
            Assert.Equal(0.1, tags["sampler.param"]);
        }
    }
}
