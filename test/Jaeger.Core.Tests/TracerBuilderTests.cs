using System;
using Jaeger.Core.Metrics;
using Jaeger.Core.Propagation;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Jaeger.Core.Transport;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class TracerBuilderTests
    {
        private readonly Tracer.Builder _baseBuilder;
        private readonly IReporter _mockReporter;
        private readonly string _serviceName;
        private readonly ISampler _mockSampler;
        private readonly IScopeManager _mockScopeManager;
        private readonly IPropagationRegistry _mockPropagationRegistry;
        private readonly IMetrics _mockMetrics;
        private readonly ITransport _mockTransport;

        public TracerBuilderTests()
        {
            _mockReporter = Substitute.For<IReporter>();
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
            Assert.Equal(stringTagValue, tracer.Tags[stringTagName]);
            Assert.Equal(boolTagValue, tracer.Tags[boolTagName]);
            Assert.Equal(doubleTagValue, tracer.Tags[doubleTagName]);
            Assert.Equal(intTagValue, tracer.Tags[intTagName]);
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

        [Fact]
        public void Builder_WithLoggingFactory_ShouldCallCreateLogger()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger>();
            var span = Substitute.For<IJaegerCoreSpan>();
            loggerFactory.CreateLogger<LoggingReporter>().Returns(logger);
            span.Context.IsSampled.Returns(true);

            var tracer = _baseBuilder
                .WithLoggerFactory(loggerFactory)
                .Build();

            Assert.IsType<LoggingReporter>(tracer.Reporter);
            loggerFactory.Received(1).CreateLogger<LoggingReporter>();

            tracer.ReportSpan(span);
            logger.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception, string>>());
        }
    }
}
