using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jaeger.Core.Metrics;
using Jaeger.Core.Samplers;
using Jaeger.Core.Samplers.HTTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class RemoteControlledSamplerTests : IDisposable
    {
        private const string SERVICE_NAME = "thachi mamu";

        private readonly ILoggerFactory _loggerFactory;
        private readonly ISamplingManager _samplingManager;
        private readonly ISampler _initialSampler;
        private readonly IMetrics _metrics;

        private RemoteControlledSampler _undertest;

        public RemoteControlledSamplerTests()
        {
            _loggerFactory = new NullLoggerFactory();

            _samplingManager = Substitute.For<ISamplingManager>();
            _initialSampler = Substitute.For<ISampler>();

            _metrics = new MetricsImpl(new InMemoryMetricsFactory());
            _undertest = new RemoteControlledSampler.Builder(SERVICE_NAME)
                .WithSamplingManager(_samplingManager)
                .WithInitialSampler(_initialSampler)
                .WithMetrics(_metrics)
                .Build();
        }
        public void Dispose()
        {
            _undertest.Close();
        }

        [Fact]
        public void TestUpdateToProbabilisticSampler()
        {
            double samplingRate = 0.55;
            SamplingStrategyResponse probabilisticResponse = new SamplingStrategyResponse(
                new ProbabilisticSamplingStrategy(samplingRate), null, null);
            _samplingManager.GetSamplingStrategyAsync(SERVICE_NAME).Returns(probabilisticResponse);

            _undertest.UpdateSampler();

            Assert.Equal(new ProbabilisticSampler(samplingRate), _undertest.Sampler);
        }

        [Fact]
        public void TestUpdateToRateLimitingSampler()
        {
            short tracesPerSecond = 22;
            SamplingStrategyResponse rateLimitingResponse = new SamplingStrategyResponse(null,
                new RateLimitingSamplingStrategy(tracesPerSecond), null);
            _samplingManager.GetSamplingStrategyAsync(SERVICE_NAME).Returns(rateLimitingResponse);

            _undertest.UpdateSampler();

            Assert.Equal(new RateLimitingSampler(tracesPerSecond), _undertest.Sampler);
        }

        [Fact]
        public void TestUpdateToPerOperationSamplerReplacesProbabilisticSampler()
        {
            var operationToSampler = new List<PerOperationSamplingParameters>();
            operationToSampler.Add(new PerOperationSamplingParameters("operation",
                new ProbabilisticSamplingStrategy(0.1)));
            OperationSamplingParameters parameters = new OperationSamplingParameters(0.11, 0.22, operationToSampler);
            SamplingStrategyResponse response = new SamplingStrategyResponse(null,
                null, parameters);
            _samplingManager.GetSamplingStrategyAsync(SERVICE_NAME).Returns(response);

            _undertest.UpdateSampler();

            PerOperationSampler perOperationSampler = new PerOperationSampler(2000, parameters, _loggerFactory);
            ISampler actualSampler = _undertest.Sampler;
            Assert.Equal(perOperationSampler, actualSampler);
        }

        [Fact(Skip="Does not yet work because it would require PerOperationSampler.Update to be virtual and calling virtual members in constructors doesn't work properly with NSubstitute (This is not recommended anyway)")]
        public async Task TestUpdatePerOperationSamplerUpdatesExistingPerOperationSampler()
        {
            OperationSamplingParameters parameters = new OperationSamplingParameters(1, 1, new List<PerOperationSamplingParameters>());
            PerOperationSampler perOperationSampler = Substitute.ForPartsOf<PerOperationSampler>(10, parameters, NullLoggerFactory.Instance);

            _samplingManager.GetSamplingStrategyAsync(SERVICE_NAME)
                .Returns(new SamplingStrategyResponse(null, null, parameters));

            _undertest = new RemoteControlledSampler.Builder(SERVICE_NAME)
                .WithSamplingManager(_samplingManager)
                .WithInitialSampler(perOperationSampler)
                .WithMetrics(_metrics)
                .Build();

            _undertest.UpdateSampler();
            await Task.Delay(20);
            //updateSampler is hit once automatically because of the pollTimer
            perOperationSampler.Received(2).Update(parameters);
        }

        [Fact]
        public void TestNullResponse()
        {
            _samplingManager.GetSamplingStrategyAsync(SERVICE_NAME).Returns(new SamplingStrategyResponse(null, null, null));
            _undertest.UpdateSampler();
            Assert.Equal(_initialSampler, _undertest.Sampler);
        }

        [Fact]
        public void TestUnparseableResponse()
        {
            _samplingManager.GetSamplingStrategyAsync(SERVICE_NAME).Throws(new InvalidOperationException("test"));
            _undertest.UpdateSampler();
            Assert.Equal(_initialSampler, _undertest.Sampler);
        }

        [Fact]
        public void TestSample()
        {
            _undertest.Sample("op", new TraceId(1L));
            _initialSampler.Received(1).Sample("op", new TraceId(1L));
        }

        [Fact]
        public void TestEquals()
        {
            RemoteControlledSampler i2 = new RemoteControlledSampler.Builder(SERVICE_NAME)
                .WithSamplingManager(_samplingManager)
                .WithInitialSampler(Substitute.For<ISampler>())
                .WithMetrics(_metrics)
                .Build();

            Assert.Equal(_undertest, _undertest);
            Assert.NotEqual(_undertest, _initialSampler);
            Assert.NotEqual(_undertest, i2);
            Assert.NotEqual(i2, _undertest);
        }

        [Fact]
        public void TestDefaultProbabilisticSampler()
        {
            _undertest = new RemoteControlledSampler.Builder(SERVICE_NAME)
                .WithSamplingManager(_samplingManager)
                .WithMetrics(_metrics)
                .Build();

            Assert.Equal(new ProbabilisticSampler(0.001), _undertest.Sampler);
        }
    }
}
