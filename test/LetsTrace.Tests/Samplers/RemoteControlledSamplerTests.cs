using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LetsTrace.Metrics;
using LetsTrace.Samplers;
using LetsTrace.Samplers.HTTP;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace LetsTrace.Tests.Samplers
{
    public class RemoteControlledSamplerTests
    {
        private readonly string _serivceName;
        private readonly ISamplingManager _mockSamplingManager;
        private readonly ILoggerFactory _mockLoggerFactory;
        private readonly ILogger _mockLogger;
        private readonly ISampler _mockSampler;
        private readonly IMetrics _mockMetrics;
        private readonly ISamplerFactory _mockSamplerFactory;
        private readonly int _pollingIntervalMs;
        private readonly Func<Action, int, CancellationToken, Task> _mockPollTimer;
        private RemoteControlledSampler _testingSampler;

        public RemoteControlledSamplerTests()
        {
            _serivceName = "testService";
            _mockSamplingManager = Substitute.For<ISamplingManager>();
            _mockLoggerFactory = Substitute.For<ILoggerFactory>();
            _mockSampler = Substitute.For<ISampler>();
            _mockMetrics = Substitute.For<IMetrics>();
            _pollingIntervalMs = 5000;
            _mockSamplerFactory = Substitute.For<ISamplerFactory>();
            _mockLogger = Substitute.For<ILogger<RemoteControlledSampler>>();
            _mockPollTimer = Substitute.For<Func<Action, int, CancellationToken, Task>>();

            _mockLoggerFactory.CreateLogger<RemoteControlledSampler>().Returns(_mockLogger);

            _testingSampler = new RemoteControlledSampler(
                _serivceName,
                _mockSamplingManager,
                _mockLoggerFactory,
                _mockMetrics,
                _mockSampler,
                _mockSamplerFactory,
                _pollingIntervalMs,
                _mockPollTimer
            );

        }

        /* TODO: Add tests for Update(PerOperationSamplingStrategies) including PerOperationStrategies */

        [Fact]
        public void UpdatePerOperationSampler_ShouldUpdateParams_WhenSamplerIsAlreadyPerOp()
        {
            var perOpSampler = new PerOperationSampler(10, 0.5, 5, _mockLoggerFactory);
            _testingSampler = new RemoteControlledSampler(
                _serivceName,
                _mockSamplingManager,
                _mockLoggerFactory,
                _mockMetrics,
                perOpSampler,
                _mockSamplerFactory,
                _pollingIntervalMs,
                _mockPollTimer
            );
            var strategies = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = 0.5,
                DefaultLowerBoundTracesPerSecond = 10,
                PerOperationStrategies = new List<OperationSamplingStrategy>()
            };

            _testingSampler.UpdatePerOperationSampler(strategies);

            _mockMetrics.Received(1).SamplerUpdated.Inc(Arg.Is<long>(d => d == 1));
        }

        [Fact]
        public void UpdatePerOperationSampler_ShouldCreateNewPerOpSampler_WhenSamplerIsNotAlreadyPerOp()
        {
            var strategies = new PerOperationSamplingStrategies
            {
                DefaultSamplingProbability = 0.5,
                DefaultLowerBoundTracesPerSecond = 10,
                PerOperationStrategies = new List<OperationSamplingStrategy>()
            };

            _testingSampler.UpdatePerOperationSampler(strategies);

            _mockMetrics.Received(0).SamplerUpdated.Inc(Arg.Is<long>(d => d == 1));
            _mockSamplerFactory.Received(1).NewPerOperationSampler(
                Arg.Is<int>(mo => mo == 2000),
                Arg.Is<double>(dsp => dsp == strategies.DefaultSamplingProbability),
                Arg.Is<double>(dlbtps => dlbtps == strategies.DefaultLowerBoundTracesPerSecond),
                Arg.Is<ILoggerFactory>(lf => lf == _mockLoggerFactory));
        }

        [Fact]
        public void IsSampled_ShouldCallSampler()
        {
            var op = "op";
            var traceId = new TraceId(452);

            _mockSampler.IsSampled(Arg.Is<TraceId>(tid => tid == traceId), Arg.Is<string>(on => on == op))
                .Returns((true, new Dictionary<string, Field>()));

            var isSampled = _testingSampler.IsSampled(traceId, op);

            Assert.True(isSampled.Sampled);
            _mockSampler.Received(1).IsSampled(Arg.Any<TraceId>(), Arg.Any<string>());
        }

        [Fact]
        public void UpdateSampler_ShouldCatchAndIndicateException()
        {
            _mockSamplingManager.GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName)).Throws(new Exception());

            _testingSampler.UpdateSampler();

            _mockSamplingManager.Received(1).GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName));
            _mockMetrics.Received(1).SamplerQueryFailure.Inc(Arg.Is<long>(delta => delta == 1));
            _mockMetrics.Received(0).SamplerRetrieved.Inc(Arg.Any<long>());
        }

        [Fact]
        public void UpdateSampler_ShouldHandleOperationSampling()
        {
            var ssResponse = new SamplingStrategyResponse
            {
                OperationSampling = new PerOperationSamplingStrategies
                {
                    DefaultSamplingProbability = 0.5,
                    DefaultLowerBoundTracesPerSecond = 10,
                    PerOperationStrategies = new List<OperationSamplingStrategy>()
                }
            };
            _mockSamplingManager.GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName)).Returns(ssResponse);

            _testingSampler.UpdateSampler();

            _mockSamplingManager.Received(1).GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName));
            _mockMetrics.Received(1).SamplerRetrieved.Inc(Arg.Any<long>());
            _mockSamplerFactory.Received(1).NewPerOperationSampler(
                Arg.Is<int>(mo => mo == 2000),
                Arg.Is<double>(dsp => dsp == ssResponse.OperationSampling.DefaultSamplingProbability),
                Arg.Is<double>(dlbtps => dlbtps == ssResponse.OperationSampling.DefaultLowerBoundTracesPerSecond),
                Arg.Is<ILoggerFactory>(lf => lf == _mockLoggerFactory));
        }

        [Fact]
        public void UpdateSampler_ShouldHandleRateLimSampling()
        {
            var ssResponse = new SamplingStrategyResponse
            {
                RateLimitingSampling = new RateLimitingSamplingStrategy { MaxTracesPerSecond = 10 }
            };
            _mockSamplingManager.GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName)).Returns(ssResponse);

            _testingSampler.UpdateSampler();

            _mockSamplingManager.Received(1).GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName));
            _mockMetrics.Received(1).SamplerRetrieved.Inc(Arg.Any<long>());
            _mockSamplerFactory.Received(1)
                .NewRateLimitingSampler(Arg.Is<short>(mt => mt == ssResponse.RateLimitingSampling.MaxTracesPerSecond));
        }

        [Fact]
        public void UpdateSampler_ShouldHandleProbSampling()
        {
            var ssResponse = new SamplingStrategyResponse
            {
                ProbabilisticSampling = new ProbabilisticSamplingStrategy { SamplingRate = 0.75 }
            };
            _mockSamplingManager.GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName)).Returns(ssResponse);

            _testingSampler.UpdateSampler();

            _mockSamplingManager.Received(1).GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName));
            _mockMetrics.Received(1).SamplerRetrieved.Inc(Arg.Any<long>());
            _mockSamplerFactory.Received(1)
                .NewProbabilisticSampler(Arg.Is<double>(sr => sr == ssResponse.ProbabilisticSampling.SamplingRate));
        }

        [Fact]
        public void UpdateSampler_ShouldHandleNoMatchingStrategy()
        {
            //var expectedLogMessage = "No strategy present in response. Not updating sampler.";
            var ssResponse = new SamplingStrategyResponse();
            _mockSamplingManager.GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName)).Returns(ssResponse);

            _testingSampler.UpdateSampler();

            _mockSamplerFactory.Received(0).NewRateLimitingSampler(Arg.Any<short>());
            _mockSamplerFactory.Received(0).NewProbabilisticSampler(Arg.Any<double>());
            _mockMetrics.Received(0).SamplerUpdated.Inc(Arg.Is<long>(d => d == 1));
            _mockMetrics.Received(1).SamplerParsingFailure.Inc(Arg.Is<long>(d => d == 1));
            // cannot mock extension methods :/ _mockLogger.Received(1).LogError(Arg.Is<string>(lm => lm == expectedLogMessage));
            //_mockLogger.Received(1).Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<string>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void UpdateRateLimitingOrProbabilisticSampler_ShouldNotUpdateWhenSamplersAreTheSame()
        {
            var ssResponse = new SamplingStrategyResponse
            {
                RateLimitingSampling = new RateLimitingSamplingStrategy { MaxTracesPerSecond = 10 }
            };
            _mockSamplingManager.GetSamplingStrategy(Arg.Is<string>(sn => sn == _serivceName)).Returns(ssResponse);
            _mockSamplerFactory.NewRateLimitingSampler(Arg.Any<short>()).Returns(_mockSampler);

            _testingSampler.UpdateSampler();

            _mockSamplerFactory.Received(1)
                .NewRateLimitingSampler(Arg.Is<short>(mt => mt == ssResponse.RateLimitingSampling.MaxTracesPerSecond));
            _mockMetrics.Received(0).SamplerUpdated.Inc(Arg.Is<long>(d => d == 1));
        }

        [Fact]
        public void Dispose_ShouldCancelToken()
        {
            var passedInPollingInt = 0;
            var passedInCancelToken = new CancellationToken();
            _mockPollTimer(Arg.Any<Action>(), Arg.Do<int>(i => passedInPollingInt = i),
                Arg.Do<CancellationToken>(ct => passedInCancelToken = ct));

            _testingSampler = new RemoteControlledSampler(
                _serivceName,
                _mockSamplingManager,
                _mockLoggerFactory,
                _mockMetrics,
                _mockSampler,
                _mockSamplerFactory,
                _pollingIntervalMs,
                _mockPollTimer
            );
            _testingSampler.Dispose();

            Assert.Equal(_pollingIntervalMs, passedInPollingInt);
            Assert.True(passedInCancelToken.IsCancellationRequested);
        }

        [Fact] public void Equals_ShouldBeTrue_WhenItsTheSameObject()
        {
            Assert.True(_testingSampler.Equals(_testingSampler));
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenTheSamplerIsTheSame()
        {
            var equalSampler = new RemoteControlledSampler(
                _serivceName,
                _mockSamplingManager,
                _mockLoggerFactory,
                _mockMetrics,
                _mockSampler,
                _mockSamplerFactory,
                _pollingIntervalMs,
                _mockPollTimer
            );

            Assert.True(_testingSampler.Equals(equalSampler));
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenItsNull()
        {
            Assert.False(_testingSampler.Equals(null));
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenItsNotARemoteControlledSampler()
        {
            var notRCSampler = new ConstSampler(true);

            Assert.False(_testingSampler.Equals(notRCSampler));
        }

        [Fact]
        public async void PollTimer_DelaysAndCallsUpdateFunc()
        {
            var updateFunc = Substitute.For<Action>();
            var pollingIntervalMs = 1000;
            var cts = new CancellationTokenSource();
            cts.CancelAfter(2600);

            await RemoteControlledSampler.PollTimer(updateFunc, pollingIntervalMs, cts.Token);

            updateFunc.Received(3)();
        }
    }
}

