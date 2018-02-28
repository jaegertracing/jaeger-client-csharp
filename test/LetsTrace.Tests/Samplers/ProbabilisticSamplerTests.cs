using System;
using System.Collections.Generic;
using LetsTrace.Samplers;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class ProbabilisticSamplerTests
    {
        [Fact]
        public void ProbabilisticSampler_Constructor_ShouldThrowIfArgumentIsBelowRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ProbabilisticSampler(-1));
            Assert.Equal((double)-1, ex.ActualValue);
            Assert.Equal("samplingRate", ex.ParamName);
            Assert.True(ex.Message.Contains("sampling rate must be between 0.0 and 1.0"));
        }

        [Fact]
        public void ProbabilisticSampler_Constructor_ShouldThrowIfArgumentIsAboveRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ProbabilisticSampler(1.1));
            Assert.Equal(1.1, ex.ActualValue);
            Assert.Equal("samplingRate", ex.ParamName);
            Assert.True(ex.Message.Contains("sampling rate must be between 0.0 and 1.0"));
        }

        [Fact]
        public void ProbabilisticSampler_Constructor_ShouldSetSamplingRate()
        {
            var samplingRate = 0.5;
            var sampler = new ProbabilisticSampler(samplingRate);
            Assert.Equal(samplingRate, sampler.SamplingRate);
        }

        [Fact]
        public void ProbabilisticSampler_IsSampled()
        {
            var middleId = 9223372036854775807;

            var samplingRate = 0.5;
            var expectedTags = new Dictionary<string, Field> {
                { Constants.SamplerTypeTagKey, new Field<string> { Value = Constants.SamplerTypeConst } },
                { Constants.SamplerParamTagKey, new Field<double> { Value = samplingRate } }
            };
            var sampler = new ProbabilisticSampler(samplingRate);
            var isSampled = sampler.IsSampled(new TraceId { Low = (UInt64) middleId + 10 }, "op");

            Assert.Equal(expectedTags, isSampled.Tags);
            Assert.False(isSampled.Sampled);

            isSampled = sampler.IsSampled(new TraceId { Low = (UInt64) middleId - 20 }, "op");
            Assert.Equal(expectedTags, isSampled.Tags);
            Assert.True(isSampled.Sampled);
        }
    }
}
