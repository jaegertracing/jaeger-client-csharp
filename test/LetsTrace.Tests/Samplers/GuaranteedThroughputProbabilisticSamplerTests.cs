using System;
using System.Collections.Generic;
using LetsTrace.Samplers;
using NSubstitute;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class GuaranteedThroughputProbabilisticSamplerTests
    {
        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_Constructor_ThrowsIfProbabilisticSamplerIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RateLimitingSampler(1.0, null));
            Assert.Equal("rateLimiter", ex.ParamName);
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_Constructor_ThrowsIfRateLimitingSamplerIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RateLimitingSampler(1.0, null));
            Assert.Equal("rateLimiter", ex.ParamName);
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_UsesProbabilisticSampler()
        {
            var probabilisticSampler = Substitute.For<IProbabilisticSampler>();
            var rateLimitingSampler = Substitute.For<IRateLimitingSampler>();
            var traceId = new TraceId(1);
            var operationName = "op";
            var sampler = new GuaranteedThroughputProbabilisticSampler(probabilisticSampler, rateLimitingSampler);

            probabilisticSampler.IsSampled(Arg.Is<TraceId>(t => t == traceId), Arg.Is<string>(o => o == operationName)).Returns((true, new Dictionary<string, object>()));

            sampler.IsSampled(traceId, operationName);
            sampler.Dispose();

            probabilisticSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            rateLimitingSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            probabilisticSampler.Received(1).Dispose();
            rateLimitingSampler.Received(1).Dispose();
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_FallsBackToRateLimitingSampler()
        {
            var probabilisticSampler = Substitute.For<IProbabilisticSampler>();
            var rateLimitingSampler = Substitute.For<IRateLimitingSampler>();
            var traceId = new TraceId(1);
            var operationName = "op";
            var sampler = new GuaranteedThroughputProbabilisticSampler(probabilisticSampler, rateLimitingSampler);

            probabilisticSampler.IsSampled(Arg.Is<TraceId>(t => t == traceId), Arg.Is<string>(o => o == operationName)).Returns((false, new Dictionary<string, object>()));
            rateLimitingSampler.IsSampled(Arg.Is<TraceId>(t => t == traceId), Arg.Is<string>(o => o == operationName));

            sampler.IsSampled(traceId, operationName);

            probabilisticSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            rateLimitingSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_UsesDefaultSamplers()
        {
            var samplingRate = 0.4;
            double lowerBound = 5;

            var sampler = new GuaranteedThroughputProbabilisticSampler(samplingRate, lowerBound);

            Assert.IsType<ProbabilisticSampler>(sampler._probabilisticSampler);
            Assert.IsType<RateLimitingSampler>(sampler._rateLimitingSampler);
            Assert.Equal(samplingRate, sampler._probabilisticSampler.SamplingRate);
            Assert.Equal(lowerBound, sampler._rateLimitingSampler.MaxTracesPerSecond);
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_Update_ShouldNotCreateNewSamplersWhenTheValuesDoNotChange()
        {
            var samplingRate = 0.4;
            double lowerBound = 5;

            var sampler = new GuaranteedThroughputProbabilisticSampler(samplingRate, lowerBound);
            var updated = sampler.Update(samplingRate, lowerBound);

            Assert.False(updated);
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_Update_ShouldCreateNewSamplersWhenTheValuesChange()
        {
            var samplingRate = 0.4;
            double lowerBound = 5;

            var sampler = new GuaranteedThroughputProbabilisticSampler(0.2, 4);
            var updated = sampler.Update(samplingRate, lowerBound);

            Assert.True(updated);
            Assert.Equal(samplingRate, sampler._probabilisticSampler.SamplingRate);
            Assert.Equal(lowerBound, sampler._rateLimitingSampler.MaxTracesPerSecond);
        }
    }
}
