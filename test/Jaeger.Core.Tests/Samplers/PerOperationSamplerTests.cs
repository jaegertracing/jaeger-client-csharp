using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jaeger.Core.Samplers;
using Jaeger.Core.Samplers.HTTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class PerOperationSamplerTests : IDisposable
    {
        private const double SamplingRate = 0.31415;
        private const double DefaultSamplingProbability = 0.512;
        private const double DefaultLowerBoundTracesPerSecond = 2.0;
        private const int MaxOperations = 100;
        private const int DoublePrecision = 2;
        private const string operation = "some OPERATION";
        private static readonly TraceId TraceId = new TraceId(1L);

        private static readonly IReadOnlyDictionary<string, object> EmptyTags = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

        private readonly ILoggerFactory _loggerFactory;
        private readonly ProbabilisticSampler _defaultProbabilisticSampler;
        private readonly Dictionary<string, GuaranteedThroughputSampler> _operationToSamplers = new Dictionary<string, GuaranteedThroughputSampler>();
        private PerOperationSampler _undertest;

        public PerOperationSamplerTests()
        {
            _loggerFactory = new NullLoggerFactory();
            _defaultProbabilisticSampler = Substitute.ForPartsOf<ProbabilisticSampler>(0);

            _undertest = new PerOperationSampler(MaxOperations, _operationToSamplers, _defaultProbabilisticSampler,
                DefaultLowerBoundTracesPerSecond, _loggerFactory);

            _defaultProbabilisticSampler.Sample(operation, TraceId)
                .Returns(new SamplingStatus(true, EmptyTags));

            _defaultProbabilisticSampler.SamplingRate.Returns(DefaultSamplingProbability);
        }

        public void Dispose()
        {
            _undertest.Close();
        }

        [Fact]
        public void TestFallbackToDefaultProbabilisticSampler()
        {
            _undertest = new PerOperationSampler(0, _operationToSamplers, _defaultProbabilisticSampler,
                DefaultLowerBoundTracesPerSecond, _loggerFactory);
            SamplingStatus samplingStatus = _undertest.Sample(operation, TraceId);
            Assert.True(samplingStatus.IsSampled);

            _defaultProbabilisticSampler.Received(1).Sample(operation, TraceId);
        }

        [Fact]
        public void TestCreateGuaranteedSamplerOnUnseenOperation()
        {
            string newOperation = "new OPERATION";
            _undertest.Sample(newOperation, TraceId);
            Assert.Equal(new GuaranteedThroughputSampler(DefaultSamplingProbability,
                                                         DefaultLowerBoundTracesPerSecond),
                         _operationToSamplers[newOperation]);
        }

        [Fact]
        public void TestPerOperationSamplerWithKnownOperation()
        {
            GuaranteedThroughputSampler sampler = Substitute.ForPartsOf<GuaranteedThroughputSampler>(0, 0);
            _operationToSamplers.Add(operation, sampler);

            sampler.Sample(operation, TraceId)
                .Returns(new SamplingStatus(true, EmptyTags));

            SamplingStatus samplingStatus = _undertest.Sample(operation, TraceId);
            Assert.True(samplingStatus.IsSampled);
            sampler.Received(1).Sample(operation, TraceId);
            //verifyNoMoreInteractions(_defaultProbabilisticSampler);
        }

        [Fact]
        public void TestUpdate()
        {
            GuaranteedThroughputSampler guaranteedThroughputSampler = Substitute.ForPartsOf<GuaranteedThroughputSampler>(0, 0);
            _operationToSamplers.Add(operation, guaranteedThroughputSampler);

            var perOperationSamplingParameters = new PerOperationSamplingParameters(operation, new ProbabilisticSamplingStrategy(SamplingRate));
            var parametersList = new List<PerOperationSamplingParameters>();
            parametersList.Add(perOperationSamplingParameters);

            var parameters = new OperationSamplingParameters(DefaultSamplingProbability, DefaultLowerBoundTracesPerSecond, parametersList);

            Assert.True(_undertest.Update(parameters));
            guaranteedThroughputSampler.Received(1).Update(SamplingRate, DefaultLowerBoundTracesPerSecond);
            //verifyNoMoreInteractions(guaranteedThroughputSampler);
        }

        [Fact]
        public void TestNoopUpdate()
        {
            ProbabilisticSampler defaultProbabilisticSampler = new ProbabilisticSampler(DefaultSamplingProbability);
            double operationSamplingRate = 0.23;
            _operationToSamplers.Add(operation, new GuaranteedThroughputSampler(operationSamplingRate,
                DefaultLowerBoundTracesPerSecond));
            _undertest = new PerOperationSampler(MaxOperations, _operationToSamplers, defaultProbabilisticSampler,
                DefaultLowerBoundTracesPerSecond, _loggerFactory);

            var parametersList = new List<PerOperationSamplingParameters>();
            parametersList.Add(new PerOperationSamplingParameters(operation, new ProbabilisticSamplingStrategy(operationSamplingRate)));

            var parameters = new OperationSamplingParameters(DefaultSamplingProbability, DefaultLowerBoundTracesPerSecond, parametersList);

            Assert.False(_undertest.Update(parameters));
            Assert.Equal(_operationToSamplers, _undertest.OperationNameToSampler);
            Assert.Equal(DefaultLowerBoundTracesPerSecond, _undertest.LowerBound, DoublePrecision);
            Assert.Equal(DefaultSamplingProbability, _undertest.DefaultSampler.SamplingRate, DoublePrecision);
        }

        [Fact]
        public void TestUpdateIgnoreGreaterThanMax()
        {
            GuaranteedThroughputSampler guaranteedThroughputSampler = Substitute.ForPartsOf<GuaranteedThroughputSampler>(0, 0);
            _operationToSamplers.Add(operation, guaranteedThroughputSampler);

            PerOperationSampler undertest = new PerOperationSampler(1, _operationToSamplers,
                _defaultProbabilisticSampler, DefaultLowerBoundTracesPerSecond, _loggerFactory);

            var perOperationSamplingParameters1 = new PerOperationSamplingParameters(operation, new ProbabilisticSamplingStrategy(SamplingRate));
            var perOperationSamplingParameters2 = new PerOperationSamplingParameters("second OPERATION", new ProbabilisticSamplingStrategy(SamplingRate));
            var parametersList = new List<PerOperationSamplingParameters>();
            parametersList.Add(perOperationSamplingParameters1);
            parametersList.Add(perOperationSamplingParameters2);

            undertest.Update(new OperationSamplingParameters(DefaultSamplingProbability,
                                                             DefaultLowerBoundTracesPerSecond, parametersList));

            Assert.Single(_operationToSamplers);
            Assert.NotNull(_operationToSamplers[operation]);
        }

        [Fact]
        public void TestUpdateAddOperation()
        {
            var perOperationSamplingParameters1 = new PerOperationSamplingParameters(operation, new ProbabilisticSamplingStrategy(SamplingRate));
            var parametersList = new List<PerOperationSamplingParameters>();
            parametersList.Add(perOperationSamplingParameters1);

            _undertest.Update(new OperationSamplingParameters(DefaultSamplingProbability,
                                                             DefaultLowerBoundTracesPerSecond,
                                                             parametersList));

            Assert.Single(_operationToSamplers);
            Assert.Equal(new GuaranteedThroughputSampler(SamplingRate,
                                                         DefaultLowerBoundTracesPerSecond),
                         _operationToSamplers[operation]);
        }
    }
}
