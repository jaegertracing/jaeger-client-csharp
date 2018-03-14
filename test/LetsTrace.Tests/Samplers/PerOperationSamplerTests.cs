using System;
using LetsTrace.Samplers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class PerOperationSamplerTests
    {
        private readonly double _samplingRate;
        private readonly double _lowerBound;
        private readonly int _maxOperations;

        private readonly IGuaranteedThroughputProbabilisticSampler _mockGtpSampler;
        private readonly ILoggerFactory _mockLoggerFactory;
        private readonly IProbabilisticSampler _mockDefaultSampler;
        private readonly ISamplerFactory _mockSamplerFactory;

        private PerOperationSampler _testingSampler;

        public PerOperationSamplerTests()
        {
            _samplingRate = 1.0;
            _lowerBound = 0.5;
            _maxOperations = 3;

            _mockGtpSampler = Substitute.For<IGuaranteedThroughputProbabilisticSampler>();
            _mockLoggerFactory = Substitute.For<ILoggerFactory>();
            _mockDefaultSampler = Substitute.For<IProbabilisticSampler>();
            _mockSamplerFactory = Substitute.For<ISamplerFactory>();

            _mockSamplerFactory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == _samplingRate),
                Arg.Is<double>(x => x == _lowerBound)
            ).Returns(_mockGtpSampler);
            _mockSamplerFactory.NewProbabilisticSampler(
                Arg.Is<double>(x => x == _samplingRate)
            ).Returns(_mockDefaultSampler);

            _testingSampler = new PerOperationSampler(_maxOperations, _samplingRate, _lowerBound, _mockLoggerFactory, _mockSamplerFactory);
        }

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
        public void PerOperationSampler_Dispose()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var samplerFactory = Substitute.For<ISamplerFactory>();
            var sampler = new PerOperationSampler(10, 1.0, 1.0, loggerFactory, samplerFactory);
            sampler.Dispose();
        }

        [Fact]
        public void PerOperationSampler_IsSampled_CreatesAnReusesSamplersForOperations()
        {
            var operationName = "op";
            var traceId = new TraceId(1);

            _testingSampler.IsSampled(traceId, operationName);
            _testingSampler.IsSampled(traceId, operationName);
            _testingSampler.IsSampled(traceId, operationName);

            _mockSamplerFactory.Received(1).NewProbabilisticSampler(Arg.Any<double>());
            _mockSamplerFactory.Received(1).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            _mockGtpSampler.Received(3).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            _mockDefaultSampler.Received(0).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        [Fact]
        public void PerOperationSampler_IsSampled_UsesDefaultSampler_WhenOverMax()
        {
            var traceId = new TraceId(1);

            _testingSampler.IsSampled(traceId, "1");
            _testingSampler.IsSampled(traceId, "2");
            _testingSampler.IsSampled(traceId, "3");
            _testingSampler.IsSampled(traceId, "4");

            _mockSamplerFactory.Received(1).NewProbabilisticSampler(Arg.Any<double>());
            _mockSamplerFactory.Received(3).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            _mockGtpSampler.Received(3).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
            _mockDefaultSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        // Update(strat)
            // should update the default sampler
            // should not update the default sampler if it's the same
            // should update the samplers for each strategy
                // should replace operation samplers that already exist
                // should add a new sampler for operations that don't have a sampler yet
                // should log and not add a new sampler when we hit the max operations
    }
}
