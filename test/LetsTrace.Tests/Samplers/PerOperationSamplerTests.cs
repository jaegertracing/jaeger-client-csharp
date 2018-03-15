using System;
using System.Collections.Generic;
using LetsTrace.Samplers;
using LetsTrace.Samplers.HTTP;
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
        private readonly ILogger<PerOperationSampler> _mockLogger;
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
            _mockLogger = Substitute.For<ILogger<PerOperationSampler>>();
            _mockDefaultSampler = Substitute.For<IProbabilisticSampler>();
            _mockSamplerFactory = Substitute.For<ISamplerFactory>();

            _mockSamplerFactory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == _samplingRate),
                Arg.Is<double>(x => x == _lowerBound)
            ).Returns(_mockGtpSampler);
            _mockSamplerFactory.NewProbabilisticSampler(
                Arg.Is<double>(x => x == _samplingRate)
            ).Returns(_mockDefaultSampler);
            _mockLoggerFactory.CreateLogger<PerOperationSampler>().Returns(_mockLogger);

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
            _testingSampler.IsSampled(new TraceId(1), "op1");
            _testingSampler.IsSampled(new TraceId(1), "op2");

            _testingSampler.Dispose();

            _mockSamplerFactory.Received(2).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            _mockGtpSampler.Received(2).Dispose();
            _mockDefaultSampler.Received(1).Dispose();
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

        [Fact]
        public void PerOperationSampler_Update_ShouldUpdateTheDefaultSampler()
        {
            var strat = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = 0.75,
                PerOperationStrategies = new List<OperationSamplingStrategy>()
            };
            var newMockDefaultSampler = Substitute.For<ISampler>();
            _mockSamplerFactory.NewProbabilisticSampler(Arg.Is<double>(v => v == strat.DefaultSamplingProbability)).Returns(newMockDefaultSampler);

            var updated = _testingSampler.Update(strat);

            Assert.True(updated);
            _mockSamplerFactory.Received(1).NewProbabilisticSampler(Arg.Is<double>(v => v == strat.DefaultSamplingProbability));
        }

        [Fact]
        public void PerOperationSampler_Update_ShouldNotUpdateTheDefaultSamplerWhenAlreadySame()
        {
            var strat = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = _samplingRate,
                PerOperationStrategies = new List<OperationSamplingStrategy>()
            };

            var updated = _testingSampler.Update(strat);

            Assert.False(updated);
            _mockSamplerFactory.Received(2).NewProbabilisticSampler(Arg.Is<double>(v => v == strat.DefaultSamplingProbability));
        }


        [Fact]
        public void PerOperationSampler_Update_ShouldReplaceOperationSamplersThatAlreadyExist()
        {
            var strat = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = _samplingRate,
                DefaultLowerBoundTracesPerSecond = 0.7,
                PerOperationStrategies = new List<OperationSamplingStrategy>
                {
                    new OperationSamplingStrategy
                    {
                        Operation = "op1",
                        ProbabilisticSampling = new ProbabilisticSamplingStrategy
                        {
                            SamplingRate = 0.25
                        }
                    },
                    new OperationSamplingStrategy
                    {
                        Operation = "op2",
                        ProbabilisticSampling = new ProbabilisticSamplingStrategy
                        {
                            SamplingRate = 0.35
                        }
                    }
                }
            };

            _testingSampler.IsSampled(new TraceId(1), strat.PerOperationStrategies[0].Operation);
            _testingSampler.IsSampled(new TraceId(1), strat.PerOperationStrategies[1].Operation);
            _mockGtpSampler.Update(Arg.Any<double>(), Arg.Any<double>()).Returns(true);

            var updated = _testingSampler.Update(strat);

            Assert.True(updated);
            _mockSamplerFactory.Received(2).NewGuaranteedThroughputProbabilisticSampler(Arg.Any<double>(), Arg.Any<double>());
            _mockGtpSampler.Received(1).Update(Arg.Is(strat.PerOperationStrategies[0].ProbabilisticSampling.SamplingRate), Arg.Is(strat.DefaultLowerBoundTracesPerSecond));
            _mockGtpSampler.Received(1).Update(Arg.Is(strat.PerOperationStrategies[1].ProbabilisticSampling.SamplingRate), Arg.Is(strat.DefaultLowerBoundTracesPerSecond));
        }

        [Fact]
        public void PerOperationSampler_Update_ShouldAddSamplerToOperationThatDoesntHaveOneYet()
        {
            var samplingRate2 = 0.35;

            var strat = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = _samplingRate,
                DefaultLowerBoundTracesPerSecond = 0.7,
                PerOperationStrategies = new List<OperationSamplingStrategy>
                {
                    new OperationSamplingStrategy
                    {
                        Operation = "op1",
                        ProbabilisticSampling = new ProbabilisticSamplingStrategy
                        {
                            SamplingRate = _samplingRate
                        }
                    },
                    new OperationSamplingStrategy
                    {
                        Operation = "op2",
                        ProbabilisticSampling = new ProbabilisticSamplingStrategy
                        {
                            SamplingRate = samplingRate2
                        }
                    }
                }
            };

            _mockSamplerFactory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == _samplingRate),
                Arg.Is<double>(x => x == strat.DefaultLowerBoundTracesPerSecond)
            ).Returns(_mockGtpSampler);
            _mockSamplerFactory.NewGuaranteedThroughputProbabilisticSampler(
                Arg.Is<double>(x => x == samplingRate2),
                Arg.Is<double>(x => x == strat.DefaultLowerBoundTracesPerSecond)
            ).Returns(_mockGtpSampler);

            var updated = _testingSampler.Update(strat);

            Assert.True(updated);
            _mockSamplerFactory.Received(1).NewGuaranteedThroughputProbabilisticSampler(Arg.Is(_samplingRate), Arg.Is(strat.DefaultLowerBoundTracesPerSecond));
            _mockSamplerFactory.Received(1).NewGuaranteedThroughputProbabilisticSampler(Arg.Is(samplingRate2), Arg.Is(strat.DefaultLowerBoundTracesPerSecond));
            _mockGtpSampler.Received(0).Update(Arg.Any<double>(), Arg.Any<double>());
        }

        [Fact]
        public void PerOperationSampler_Update_ShouldLogAndNotAddSamplerWhenWeHitTheMaxOps()
        {
            var expectedLogMessage = $"Exceeded the maximum number of operations ({_maxOperations}) for per operations sampling";
            var strat = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = _samplingRate,
                DefaultLowerBoundTracesPerSecond = 0.7,
                PerOperationStrategies = new List<OperationSamplingStrategy>
                {
                    new OperationSamplingStrategy
                    {
                        Operation = "op4",
                        ProbabilisticSampling = new ProbabilisticSamplingStrategy
                        {
                            SamplingRate = 0.25
                        }
                    },
                    new OperationSamplingStrategy
                    {
                        Operation = "op5",
                        ProbabilisticSampling = new ProbabilisticSamplingStrategy
                        {
                            SamplingRate = 0.35
                        }
                    }
                }
            };

            _testingSampler.IsSampled(new TraceId(1), "op1");
            _testingSampler.IsSampled(new TraceId(1), "op2");
            _testingSampler.IsSampled(new TraceId(1), "op3");

            var updated = _testingSampler.Update(strat);

            Assert.False(updated);
            _mockSamplerFactory.Received(3).NewGuaranteedThroughputProbabilisticSampler(Arg.Is(_samplingRate), Arg.Is(_lowerBound));
            _mockGtpSampler.Received(0).Update(Arg.Any<double>(), Arg.Any<double>());
            // cannot mock extension methods :/ _mockLogger.Received(1).LogError(Arg.Is<string>(lm => lm == expectedLogMessage));
            //_mockLogger.Received(1).Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<string>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception, string>>());
        }
    }
}
