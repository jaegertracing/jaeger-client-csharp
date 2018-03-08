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
            var operationName = "op";
            var sampler = new GuaranteedThroughputProbabilisticSampler(probabilisticSampler, rateLimitingSampler);

            probabilisticSampler.IsSampled(Arg.Any<TraceId>(), Arg.Is<string>(o => o == operationName)).Returns((true, new Dictionary<string, Field>()));

            sampler.IsSampled(new TraceId(), operationName);

            probabilisticSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            rateLimitingSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_FallsBackToRateLimitingSampler()
        {
            var probabilisticSampler = Substitute.For<IProbabilisticSampler>();
            var rateLimitingSampler = Substitute.For<IRateLimitingSampler>();
            var operationName = "op";
            var sampler = new GuaranteedThroughputProbabilisticSampler(probabilisticSampler, rateLimitingSampler);

            probabilisticSampler.IsSampled(Arg.Any<TraceId>(), Arg.Is<string>(o => o == operationName)).Returns((false, new Dictionary<string, Field>()));
            rateLimitingSampler.IsSampled(Arg.Any<TraceId>(), Arg.Is<string>(o => o == operationName));

            sampler.IsSampled(new TraceId(), operationName);

            probabilisticSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            rateLimitingSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }
    }
}
