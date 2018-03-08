using System;
using LetsTrace.Metrics;
using LetsTrace.Propagation;
using LetsTrace.Reporters;
using LetsTrace.Samplers;
using LetsTrace.Transport;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
using Xunit;

namespace LetsTrace.Tests
{
    public class TracerBuilderTests
    {
        private readonly Tracer.Builder _baseBuilder;
        private readonly IReporter _mockReporter;
        private readonly string _operationName;
        private readonly string _serviceName;
        private readonly ISampler _mockSampler;
        private readonly IScopeManager _mockScopeManager;
        private readonly IPropagationRegistry _mockPropagationRegistry;
        private readonly IMetrics _mockMetrics;
        private readonly ITransport _mockTransport;

        public TracerBuilderTests()
        {
            _mockReporter = Substitute.For<IReporter>();
            _operationName = "GET::api/values/";
            _serviceName = "testingService";
            _mockSampler = Substitute.For<ISampler>();
            _mockScopeManager = Substitute.For<IScopeManager>();
            _mockPropagationRegistry = Substitute.For<IPropagationRegistry>();
            _mockMetrics = Substitute.For<IMetrics>();
            _mockTransport = Substitute.For<ITransport>();

            _baseBuilder = new Tracer.Builder(_serviceName);
        }

        [Fact]
        public void Builder_Constructor_ShouldThrowWhenServiceNameIsNull()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Tracer.Builder(null).Build());
            Assert.Equal("serviceName", ex.ParamName);
        }

        [Fact]
        public void Builder_ShouldPassAlongAllTracerConstructorVars()
        {
            var stringTagName = "stringTagName";
            var stringTagValue = "stringTagValue";
            var boolTagName = "boolTagName";
            var boolTagValue = true;
            var doubleTagName = "doubleTagName";
            var doubleTagValue = 3D;
            var intTagName = "intTagName";
            var intTagValue = 16;

            var tracer = _baseBuilder
                .WithReporter(_mockReporter)
                .WithSampler(_mockSampler)
                .WithScopeManager(_mockScopeManager)
                .WithTag(stringTagName, stringTagValue)
                .WithTag(boolTagName, boolTagValue)
                .WithTag(doubleTagName, doubleTagValue)
                .WithTag(intTagName, intTagValue)
                .WithPropagationRegistry(_mockPropagationRegistry)
                .WithMetrics(_mockMetrics)
                .Build();

            Assert.Equal(_serviceName, tracer.ServiceName);
            Assert.Equal(stringTagValue, tracer.Tags[stringTagName].StringValue);
            Assert.Equal(boolTagValue, tracer.Tags[boolTagName].ValueAs<bool>());
            Assert.Equal(doubleTagValue, tracer.Tags[doubleTagName].ValueAs<double>());
            Assert.Equal(intTagValue, tracer.Tags[intTagName].ValueAs<int>());
            Assert.Equal(_mockScopeManager, tracer.ScopeManager);
            Assert.Equal(_mockPropagationRegistry, tracer.PropagationRegistry);
            Assert.Equal(_mockSampler, tracer.Sampler);
            Assert.Equal(_mockReporter, tracer.Reporter);
            Assert.Equal(_mockMetrics, tracer.Metrics);
        }

        [Fact]
        public void Builder_ShouldUseOpenTracingScopeManagerWhenScopeManagerIsNull()
        {
            Assert.True(_baseBuilder.Build().ScopeManager is AsyncLocalScopeManager);
        }

        [Fact]
        public void Builder_ShouldSetupDefaultInjectorsAndExtractors()
        {
            var scopeManager = Substitute.For<IScopeManager>();

            var tracer = _baseBuilder
                .WithScopeManager(scopeManager)
                .Build();

            Assert.IsType<TextMapPropagationRegistry>(tracer.PropagationRegistry);

            var propagationRegistry = (TextMapPropagationRegistry)tracer.PropagationRegistry;
            Assert.Contains(propagationRegistry._injectors, i => i.Key == BuiltinFormats.TextMap.ToString());
            Assert.Contains(propagationRegistry._injectors, i => i.Key == BuiltinFormats.HttpHeaders.ToString());
            Assert.Contains(propagationRegistry._extractors, i => i.Key == BuiltinFormats.TextMap.ToString());
            Assert.Contains(propagationRegistry._extractors, i => i.Key == BuiltinFormats.HttpHeaders.ToString());
        }

        [Fact]
        public void Builder_ShouldUsePassedInTransportAndMetricsForReporter()
        {
            var tracer = _baseBuilder
                .WithTransport(_mockTransport)
                .WithMetrics(_mockMetrics)
                .Build();

            Assert.Equal(_mockTransport, ((RemoteReporter)tracer.Reporter)._transport);
            Assert.Equal(_mockMetrics, ((RemoteReporter)tracer.Reporter)._metrics);
        }

        [Fact]
        public void Builder_ShouldUseSamplingManagerWhenSamplerIsNull()
        {
            var samplingManager = Substitute.For<ISamplingManager>();

            var tracer = _baseBuilder
                .WithSamplingManager(samplingManager)
                .Build();

            Assert.Equal(samplingManager, ((RemoteControlledSampler)tracer.Sampler)._samplingManager);
        }


        [Fact]
        public void Builder_WithMetricsFactory_ShouldCallCreateMetrics()
        {
            var metricsFactory = Substitute.For<IMetricsFactory>();
            var metrics = Substitute.For<IMetrics>();
            metricsFactory.CreateMetrics().Returns(metrics);

            var tracer = _baseBuilder
                .WithMetricsFactory(metricsFactory)
                .Build();

            Assert.Equal(metrics, tracer.Metrics);
            metricsFactory.Received(1).CreateMetrics();
        }
    }
}
