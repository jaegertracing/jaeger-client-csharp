using System;
using Jaeger.Core.Samplers;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class GuaranteedThroughputProbabilisticSamplerTests : IDisposable
    {
        private GuaranteedThroughputSampler _undertest;

        public void Dispose()
        {
            _undertest.Close();
        }

        [Fact]
        public void TestRateLimitingLowerBound()
        {
            _undertest = new GuaranteedThroughputSampler(0.0001, 1.0);

            SamplingStatus samplingStatus = _undertest.Sample("test", new TraceId(long.MaxValue));
            Assert.True(samplingStatus.IsSampled);
            var tags = samplingStatus.Tags;

            Assert.Equal(GuaranteedThroughputSampler.Type, tags[Constants.SamplerTypeTagKey]);
            Assert.Equal(0.0001, tags[Constants.SamplerParamTagKey]);
        }

        [Fact]
        public void TestProbabilityTagsOverrideRateLimitingTags()
        {
            _undertest = new GuaranteedThroughputSampler(0.999, 1.0);

            SamplingStatus samplingStatus = _undertest.Sample("test", new TraceId(0L));
            Assert.True(samplingStatus.IsSampled);
            var tags = samplingStatus.Tags;

            Assert.Equal(ProbabilisticSampler.Type, tags[Constants.SamplerTypeTagKey]);
            Assert.Equal(0.999, tags[Constants.SamplerParamTagKey]);
        }

        [Fact]
        public void TestUpdate_probabilisticSampler()
        {
            _undertest = new GuaranteedThroughputSampler(0.001, 1);

            Assert.False(_undertest.Update(0.001, 1));
            Assert.True(_undertest.Update(0.002, 1));

            SamplingStatus samplingStatus = _undertest.Sample("test", new TraceId(long.MaxValue));
            Assert.True(samplingStatus.IsSampled);
            var tags = samplingStatus.Tags;

            Assert.Equal(GuaranteedThroughputSampler.Type, tags[Constants.SamplerTypeTagKey]);
            Assert.Equal(0.002, tags[Constants.SamplerParamTagKey]);
        }

        [Fact]
        public void TestUpdate_rateLimitingSampler()
        {
            _undertest = new GuaranteedThroughputSampler(0.001, 1);

            Assert.False(_undertest.Update(0.001, 1));
            Assert.True(_undertest.Update(0.001, 0));

            SamplingStatus samplingStatus = _undertest.Sample("test", new TraceId(0L));
            Assert.True(samplingStatus.IsSampled);
            var tags = samplingStatus.Tags;

            Assert.Equal(ProbabilisticSampler.Type, tags[Constants.SamplerTypeTagKey]);
            Assert.Equal(0.001, tags[Constants.SamplerParamTagKey]);
        }
    }
}
