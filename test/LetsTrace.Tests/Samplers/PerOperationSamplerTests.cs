using System;
using LetsTrace.Samplers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class PerOperationSamplerTests
    {
        [Fact]
        public void PerOperationSampler_Constructor_ThrowsIfLoggerFactoryIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new PerOperationSampler(10, 1.0, 1.0, null, null));
            Assert.Equal("loggerFactory", ex.ParamName);
        }

        [Fact]
        public void PerOperationSampler_Constructor_ThrowsIfSamplerFactoryIsNull()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();

            var ex = Assert.Throws<ArgumentNullException>(() => new PerOperationSampler(10, 1.0, 1.0, loggerFactory, null));
            Assert.Equal("samplerFactory", ex.ParamName);
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_CreatesAnReusesSamplersForOperations()
        {
            var samplingRate = 1.0;
            var lowerBound = 0.5;
            var operationName = "op";
            var maxOperations = 3;
            var traceId = new TraceId(1);

            var gtpSampler = Substitute.For<IGuaranteedThroughputProbabilisticSampler>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var defaultSampler = Substitute.For<IProbabilisticSampler>();
            var samplerFactory = Substitute.For<ISamplerFactory>();
            samplerFactory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate),
                Arg.Is<double>(x => x == lowerBound)
            ).Returns(gtpSampler);
            samplerFactory.NewProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate)
            ).Returns(defaultSampler);

            var sampler = new PerOperationSampler(maxOperations, samplingRate, lowerBound, loggerFactory, samplerFactory);
            sampler.IsSampled(traceId, operationName);
            sampler.IsSampled(traceId, operationName);
            sampler.IsSampled(traceId, operationName);

            samplerFactory.Received(1).NewProbabilisticSampler(Arg.Any<double>());
            samplerFactory.Received(1).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            gtpSampler.Received(3).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            defaultSampler.Received(0).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        [Fact]
        public void GuaranteedThroughputProbabilisticSampler_IsSampled_UsesDefaultSampler_WhenOverMax()
        {
            var samplingRate = 1.0;
            var lowerBound = 0.5;
            var maxOperations = 3;
            var traceId = new TraceId(1);

            var gtpSampler = Substitute.For<IGuaranteedThroughputProbabilisticSampler>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var defaultSampler = Substitute.For<IProbabilisticSampler>();
            var samplerFactory = Substitute.For<ISamplerFactory>();
            samplerFactory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate),
                Arg.Is<double>(x => x == lowerBound)
            ).Returns(gtpSampler);
            samplerFactory.NewProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate)
            ).Returns(defaultSampler);

            var sampler = new PerOperationSampler(maxOperations, samplingRate, lowerBound, loggerFactory, samplerFactory);
            sampler.IsSampled(traceId, "1");
            sampler.IsSampled(traceId, "2");
            sampler.IsSampled(traceId, "3");
            sampler.IsSampled(traceId, "4");

            samplerFactory.Received(1).NewProbabilisticSampler(Arg.Any<double>());
            samplerFactory.Received(3).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            gtpSampler.Received(3).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            defaultSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }
    }
}
