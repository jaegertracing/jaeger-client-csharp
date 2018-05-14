using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core.Metrics;
using Jaeger.Core.Samplers;
using Jaeger.Core.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;
using OpenTracing.Noop;
using OpenTracing.Propagation;
using OpenTracing.Util;
using Xunit;
using static Jaeger.Core.Configuration;

namespace Jaeger.Core.Tests
{
    public class ConfigurationTests : IDisposable
    {
        private const string TestProperty = "TestProperty";

        private readonly ILoggerFactory _loggerFactory;

        public ConfigurationTests()
        {
            _loggerFactory = NullLoggerFactory.Instance;

            ClearProperties();
        }

        public void Dispose()
        {
            ClearProperties();
        }

        private void ClearProperties()
        {
            // Explicitly clear all properties
            ClearProperty(Configuration.JaegerAgentHost);
            ClearProperty(Configuration.JaegerAgentPort);
            ClearProperty(Configuration.JaegerReporterLogSpans);
            ClearProperty(Configuration.JaegerReporterMaxQueueSize);
            ClearProperty(Configuration.JaegerReporterFlushInterval);
            ClearProperty(Configuration.JaegerSamplerType);
            ClearProperty(Configuration.JaegerSamplerParam);
            ClearProperty(Configuration.JaegerSamplerManagerHostPort);
            ClearProperty(Configuration.JaegerServiceName);
            ClearProperty(Configuration.JaegerTags);
            ClearProperty(Configuration.JaegerEndpoint);
            ClearProperty(Configuration.JaegerAuthToken);
            ClearProperty(Configuration.JaegerUser);
            ClearProperty(Configuration.JaegerPassword);
            ClearProperty(Configuration.JaegerPropagation);

            ClearProperty(TestProperty);

            // Reset opentracing's global tracer
            FieldInfo field = typeof(GlobalTracer).GetField("_tracer", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(GlobalTracer.Instance, NoopTracerFactory.Create());
        }

        private void ClearProperty(string name)
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        private void SetProperty(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        [Fact]
        public void TestFromEnv()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            Assert.NotNull(Configuration.FromEnv(_loggerFactory).GetTracer());
            Assert.False(GlobalTracer.IsRegistered());
        }

        [Fact]
        public void TestSamplerConst()
        {
            SetProperty(Configuration.JaegerSamplerType, ConstSampler.Type);
            SetProperty(Configuration.JaegerSamplerParam, "1");
            SamplerConfiguration samplerConfig = SamplerConfiguration.FromEnv(_loggerFactory);
            Assert.Equal(ConstSampler.Type, samplerConfig.Type);
            Assert.Equal(1, samplerConfig.Param);
        }

        [Fact]
        public void TestSamplerConstInvalidParam()
        {
            SetProperty(Configuration.JaegerSamplerType, ConstSampler.Type);
            SetProperty(Configuration.JaegerSamplerParam, "X");
            SamplerConfiguration samplerConfig = SamplerConfiguration.FromEnv(_loggerFactory);
            Assert.Equal(ConstSampler.Type, samplerConfig.Type);
            Assert.Null(samplerConfig.Param);
        }

        [Fact]
        public void TestReporterConfiguration()
        {
            SetProperty(Configuration.JaegerReporterLogSpans, "true");
            SetProperty(Configuration.JaegerAgentHost, "MyHost");
            SetProperty(Configuration.JaegerAgentPort, "1234");
            SetProperty(Configuration.JaegerReporterFlushInterval, "500");
            SetProperty(Configuration.JaegerReporterMaxQueueSize, "1000");
            ReporterConfiguration reporterConfig = ReporterConfiguration.FromEnv(_loggerFactory);
            Assert.True(reporterConfig.LogSpans);
            Assert.Equal("MyHost", reporterConfig.SenderConfig.AgentHost);
            Assert.Equal(1234, reporterConfig.SenderConfig.AgentPort);
            Assert.Equal(TimeSpan.FromMilliseconds(500), reporterConfig.FlushInterval);
            Assert.Equal(1000, reporterConfig.MaxQueueSize);
        }

        [Fact]
        public void TestReporterConfigurationInvalidFlushInterval()
        {
            SetProperty(Configuration.JaegerReporterFlushInterval, "X");
            ReporterConfiguration reporterConfig = ReporterConfiguration.FromEnv(_loggerFactory);
            Assert.Null(reporterConfig.FlushInterval);
        }

        [Fact]
        public void TestReporterConfigurationInvalidLogSpans()
        {
            SetProperty(Configuration.JaegerReporterLogSpans, "X");
            ReporterConfiguration reporterConfig = ReporterConfiguration.FromEnv(_loggerFactory);
            Assert.False(reporterConfig.LogSpans);
        }

        [Fact]
        public void TestTracerTagslist()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            SetProperty(Configuration.JaegerTags, "testTag1=testValue1, testTag2 = testValue2");
            Tracer tracer = (Tracer)Configuration.FromEnv(_loggerFactory).GetTracer();
            Assert.Equal("testValue1", tracer.Tags["testTag1"]);
            Assert.Equal("testValue2", tracer.Tags["testTag2"]);
        }

        [Fact]
        public void TestTracerTagslistFormatError()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            SetProperty(Configuration.JaegerTags, "testTag1, testTag2 = testValue2");
            Tracer tracer = (Tracer)Configuration.FromEnv(_loggerFactory).GetTracer();
            Assert.Equal("testValue2", tracer.Tags["testTag2"]);
        }

        [Fact]
        public void TestTracerTagsSubstitutionDefault()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            SetProperty(Configuration.JaegerTags, "testTag1=${" + TestProperty + ":hello}");
            Tracer tracer = (Tracer)Configuration.FromEnv(_loggerFactory).GetTracer();
            Assert.Equal("hello", tracer.Tags["testTag1"]);
        }

        [Fact]
        public void TestTracerTagsSubstitutionSpecified()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            SetProperty(TestProperty, "goodbye");
            SetProperty(Configuration.JaegerTags, "testTag1=${" + TestProperty + ":hello}");
            Tracer tracer = (Tracer)Configuration.FromEnv(_loggerFactory).GetTracer();
            Assert.Equal("goodbye", tracer.Tags["testTag1"]);
        }

        [Fact]
        public void TestSenderWithEndpointWithoutAuthData()
        {
            SetProperty(Configuration.JaegerEndpoint, "https://jaeger-collector:14268/api/traces");
            ISender sender = Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender();
            Assert.True(sender is HttpSender);
        }

        [Fact]
        public void TestSenderWithAgentDataFromEnv()
        {
            SetProperty(Configuration.JaegerAgentHost, "jaeger-agent");
            SetProperty(Configuration.JaegerAgentPort, "6832");
            Assert.Throws<SocketException>(() => Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender());
        }

        [Fact]
        public void TestSenderBackwardsCompatibilityGettingAgentHostAndPort()
        {
            SetProperty(Configuration.JaegerAgentHost, "jaeger-agent");
            SetProperty(Configuration.JaegerAgentPort, "6832");
            Assert.Equal("jaeger-agent", Configuration.ReporterConfiguration.FromEnv(_loggerFactory).SenderConfig.AgentHost);
            Assert.Equal(6832, Configuration.ReporterConfiguration.FromEnv(_loggerFactory).SenderConfig.AgentPort);
        }

        [Fact]
        public void TestNoNullPointerOnNullSender()
        {
            var reporterConfiguration = new Configuration.ReporterConfiguration(_loggerFactory);
            Assert.Null(reporterConfiguration.SenderConfig.AgentHost);
            Assert.Null(reporterConfiguration.SenderConfig.AgentPort);
        }

        [Fact(Skip="Java is using the Builder for this and it is deprecated.")]
        public void TestCustomSender()
        {
            String endpoint = "https://custom-sender-endpoint:14268/api/traces";
            SetProperty(Configuration.JaegerEndpoint, "https://jaeger-collector:14268/api/traces");
            CustomSender customSender = new CustomSender(endpoint);
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(_loggerFactory);
            //senderConfiguration.Sender = customSender;
            Assert.Equal(endpoint, ((CustomSender)senderConfiguration.GetSender()).Endpoint);
        }

        [Fact]
        public void TestSenderWithBasicAuthUsesHttpSender()
        {
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(_loggerFactory)
                    .WithEndpoint("https://jaeger-collector:14268/api/traces")
                    .WithAuthUsername("username")
                    .WithAuthPassword("password");
            Assert.True(senderConfiguration.GetSender() is HttpSender);
        }

        [Fact]
        public void TestSenderWithAuthTokenUsesHttpSender()
        {
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(_loggerFactory)
                    .WithEndpoint("https://jaeger-collector:14268/api/traces")
                    .WithAuthToken("authToken");
            Assert.True(senderConfiguration.GetSender() is HttpSender);
        }

        [Fact]
        public void TestSenderWithAllPropertiesReturnsHttpSender()
        {
            SetProperty(Configuration.JaegerEndpoint, "https://jaeger-collector:14268/api/traces");
            SetProperty(Configuration.JaegerAgentHost, "jaeger-agent");
            SetProperty(Configuration.JaegerAgentPort, "6832");

            Assert.True(Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender() is HttpSender);
        }

        [Fact]
        public void TestPropagationB3Only()
        {
            SetProperty(Configuration.JaegerPropagation, "b3");
            SetProperty(Configuration.JaegerServiceName, "Test");

            TraceId traceId = new TraceId(1234);
            SpanId spanId = new SpanId(5678);

            TestTextMap textMap = new TestTextMap();
            SpanContext spanContext = new SpanContext(traceId, spanId, new SpanId(0), (byte)0);

            ITracer tracer = Configuration.FromEnv(_loggerFactory).GetTracer();
            tracer.Inject(spanContext, BuiltinFormats.TextMap, textMap);

            Assert.NotNull(textMap.Get("X-B3-TraceId"));
            Assert.NotNull(textMap.Get("X-B3-SpanId"));
            Assert.Null(textMap.Get("uber-trace-id"));

            SpanContext extractedContext = (SpanContext)tracer.Extract(BuiltinFormats.TextMap, textMap);
            Assert.Equal(traceId, extractedContext.TraceId);
            Assert.Equal(spanId, extractedContext.SpanId);
        }

        [Fact]
        public void TestPropagationJaegerAndB3()
        {
            SetProperty(Configuration.JaegerPropagation, "jaeger,b3");
            SetProperty(Configuration.JaegerServiceName, "Test");

            TraceId traceId = new TraceId(1234);
            SpanId spanId = new SpanId(5678);

            TestTextMap textMap = new TestTextMap();
            SpanContext spanContext = new SpanContext(traceId, spanId, new SpanId(0), (byte)0);

            ITracer tracer = Configuration.FromEnv(_loggerFactory).GetTracer();
            tracer.Inject(spanContext, BuiltinFormats.TextMap, textMap);

            Assert.NotNull(textMap.Get("uber-trace-id"));
            Assert.NotNull(textMap.Get("X-B3-TraceId"));
            Assert.NotNull(textMap.Get("X-B3-SpanId"));

            SpanContext extractedContext = (SpanContext)tracer.Extract(BuiltinFormats.TextMap, textMap);
            Assert.Equal(traceId, extractedContext.TraceId);
            Assert.Equal(spanId, extractedContext.SpanId);
        }

        [Fact]
        public void TestPropagationDefault()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");

            TestTextMap textMap = new TestTextMap();
            SpanContext spanContext = new SpanContext(new TraceId(1234), new SpanId(5678), new SpanId(0), (byte)0);

            Configuration.FromEnv(_loggerFactory).GetTracer().Inject(spanContext, BuiltinFormats.TextMap, textMap);

            Assert.NotNull(textMap.Get("uber-trace-id"));
            Assert.Null(textMap.Get("X-B3-TraceId"));
            Assert.Null(textMap.Get("X-B3-SpanId"));
        }

        [Fact]
        public void TestPropagationValidFormat()
        {
            SetProperty(Configuration.JaegerPropagation, "jaeger,invalid");
            SetProperty(Configuration.JaegerServiceName, "Test");

            TestTextMap textMap = new TestTextMap();
            SpanContext spanContext = new SpanContext(new TraceId(1234), new SpanId(5678), new SpanId(0), (byte)0);

            Configuration.FromEnv(_loggerFactory).GetTracer().Inject(spanContext, BuiltinFormats.TextMap, textMap);

            // Check that jaeger context still available even though invalid format specified
            Assert.NotNull(textMap.Get("uber-trace-id"));
        }

        [Fact]
        public void TestNoServiceName()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(null, _loggerFactory));
        }

        [Fact]
        public async Task TestDefaultTracer()
        {
            Configuration configuration = new Configuration("name", _loggerFactory);
            Assert.NotNull(configuration.GetTracer());
            Assert.NotNull(configuration.GetTracer());
            await configuration.CloseTracerAsync(CancellationToken.None);
        }

        [Fact]
        public void TestUnknownSampler()
        {
            SamplerConfiguration samplerConfiguration = new SamplerConfiguration(_loggerFactory);
            samplerConfiguration.WithType("unknown");

            Assert.Throws<NotSupportedException>(() => new Configuration("name", _loggerFactory)
                .WithSampler(samplerConfiguration)
                .GetTracer());
        }

        [Fact]
        public void TestConstSampler()
        {
            SamplerConfiguration samplerConfiguration = new SamplerConfiguration(_loggerFactory)
                .WithType(ConstSampler.Type);
            ISampler sampler = samplerConfiguration.CreateSampler("name",
                new MetricsImpl(NoopMetricsFactory.Instance));
            Assert.True(sampler is ConstSampler);
        }

        [Fact]
        public void TestProbabilisticSampler()
        {
            SamplerConfiguration samplerConfiguration = new SamplerConfiguration(_loggerFactory)
                .WithType(ProbabilisticSampler.Type);
            ISampler sampler = samplerConfiguration.CreateSampler("name",
                new MetricsImpl(NoopMetricsFactory.Instance));
            Assert.True(sampler is ProbabilisticSampler);
        }

        [Fact]
        public void TestRateLimitingSampler()
        {
            SamplerConfiguration samplerConfiguration = new SamplerConfiguration(_loggerFactory)
                .WithType(RateLimitingSampler.Type);
            ISampler sampler = samplerConfiguration.CreateSampler("name",
                new MetricsImpl(NoopMetricsFactory.Instance));
            Assert.True(sampler is RateLimitingSampler);
        }

        internal class TestTextMap : ITextMap
        {
            private Dictionary<string, string> _values = new Dictionary<string, string>();

            public void Set(String key, String value)
            {
                _values[key] = value;
            }

            public String Get(String key)
            {
                return _values.TryGetValue(key, out var value) ? value : null;
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _values.GetEnumerator();
            }
        }

        private class CustomSender : HttpSender
        {
            public string Endpoint { get; }

            public CustomSender(string endpoint)
                : base(endpoint)
            {
                Endpoint = endpoint;
            }
        }
    }
}
