using System;
using LetsTrace.Samplers;
using NSubstitute;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class PerOperationSamplerTests
    {
        [Fact]
        public void PerOperationSampler_Constructor_ThrowsIfFactoryIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new PerOperationSampler(10, 1.0, 1.0, null));
            Assert.Equal("factory", ex.ParamName);
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_CreatesAnReusesSamplersForOperations()
        {
            var samplingRate = 1.0;
            var lowerBound = 0.5;
            var operationName = "op";
            var maxOperations = 3;

            var gtpSampler = Substitute.For<IGuaranteedThroughputProbabilisticSampler>();
            var defaultSampler = Substitute.For<IProbabilisticSampler>();
            var factory = Substitute.For<ISamplerFactory>();
            factory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate),
                Arg.Is<double>(x => x == lowerBound)
            ).Returns(gtpSampler);
            factory.NewProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate)
            ).Returns(defaultSampler);

            var sampler = new PerOperationSampler(maxOperations, samplingRate, lowerBound, factory);
            sampler.IsSampled(new TraceId(), operationName);
            sampler.IsSampled(new TraceId(), operationName);
            sampler.IsSampled(new TraceId(), operationName);

            factory.Received(1).NewProbabilisticSampler(Arg.Any<double>());
            factory.Received(1).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            gtpSampler.Received(3).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            defaultSampler.Received(0).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_UsesDefaultSampler_WhenOverMax()
        {
            var samplingRate = 1.0;
            var lowerBound = 0.5;
            var maxOperations = 3;

            var gtpSampler = Substitute.For<IGuaranteedThroughputProbabilisticSampler>();
            var defaultSampler = Substitute.For<IProbabilisticSampler>();
            var factory = Substitute.For<ISamplerFactory>();
            factory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate),
                Arg.Is<double>(x => x == lowerBound)
            ).Returns(gtpSampler);
            factory.NewProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate)
            ).Returns(defaultSampler);

            var sampler = new PerOperationSampler(maxOperations, samplingRate, lowerBound, factory);
            sampler.IsSampled(new TraceId(), "1");
            sampler.IsSampled(new TraceId(), "2");
            sampler.IsSampled(new TraceId(), "3");
            sampler.IsSampled(new TraceId(), "4");

            factory.Received(1).NewProbabilisticSampler(Arg.Any<double>());
            factory.Received(3).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            gtpSampler.Received(3).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            defaultSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }
    }
}
