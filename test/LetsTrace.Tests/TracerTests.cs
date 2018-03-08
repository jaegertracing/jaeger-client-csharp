using System;
using System.Collections.Generic;
using LetsTrace.Propagation;
using LetsTrace.Reporters;
using LetsTrace.Samplers;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using OpenTracing;
using OpenTracing.Propagation;
using Xunit;

namespace LetsTrace.Tests
{
    public class TracerTests
    {
        private readonly ILetsTraceTracer _builtTracer;
        private readonly IReporter _mockReporter;
        private readonly string _operationName;
        private readonly string _serviceName;
        private readonly ISampler _mockSampler;
        private readonly IScopeManager _mockScopeManager;
        private readonly IInjector _mockInjector;
        private readonly IExtractor _mockExtractor;
        private readonly IFormat<string> _format;
        private readonly IPropagationRegistry _propagationRegistry;
        private readonly string _ip;

        public TracerTests()
        {
            _mockReporter = Substitute.For<IReporter>();
            _operationName = "GET::api/values/";
            _serviceName = "testingService";
            _mockSampler = Substitute.For<ISampler>();
            _mockScopeManager = Substitute.For<IScopeManager>();
            _mockInjector = Substitute.For<IInjector>();
            _mockExtractor = Substitute.For<IExtractor>();
            _format = new Builtin<string>("format");
            _propagationRegistry = new PropagationRegistry();
            _propagationRegistry.AddCodec(_format, _mockInjector, _mockExtractor);
            _ip = "192.168.1.1";

            _builtTracer = new Tracer.Builder(_serviceName)
                .WithReporter(_mockReporter)
                .WithSampler(_mockSampler)
                .WithScopeManager(_mockScopeManager)
                .WithPropagationRegistry(_propagationRegistry)
                .WithTag(Constants.TRACER_IP_TAG_KEY, _ip)
                .Build();
        }

        [Fact]
        public void Tracer_BuildSpan_ShouldPassItselfAndOperationNameToSpanBuilder()
        {
            _mockSampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            var span = (ILetsTraceSpan)_builtTracer.BuildSpan(_operationName).Start();

            Assert.Equal(_operationName, span.OperationName);
            Assert.Equal(_builtTracer, span.Tracer);
        }

        [Fact]
        public void Tracer_ReportSpan_ShouldPassSpanToReporter()
        {
            var span = Substitute.For<ILetsTraceSpan>();
            var context = Substitute.For<ILetsTraceSpanContext>();
            context.IsSampled.Returns(true);
            span.Context.Returns(context);

            _builtTracer.ReportSpan(span);
            _mockReporter.Received(1).Report(Arg.Any<ILetsTraceSpan>());
        }

        [Fact]
        public void Tracer_ReportSpan_ShouldNotReportWhenNotSampled()
        {
            var span = Substitute.For<ILetsTraceSpan>();
            var context = Substitute.For<ILetsTraceSpanContext>();
            context.IsSampled.Returns(false);
            span.Context.Returns(context);

            _builtTracer.ReportSpan(span);
            _mockReporter.Received(0).Report(Arg.Any<ILetsTraceSpan>());
        }

        [Fact]
        public void Tracer_ExtractAndInject_ShouldThrowWhenCodecDoesNotExist()
        {
            var carrier = "carrier, yo";
            var spanContext = Substitute.For<ILetsTraceSpanContext>();
            var format = new Builtin<string>("formatDoesNotExist");

            var ex = Assert.Throws<Exception>(() => _builtTracer.Extract(format, carrier));
            Assert.Equal($"{format} is not a supported extraction format", ex.Message);

            ex = Assert.Throws<Exception>(() => _builtTracer.Inject(spanContext, format, carrier));
            Assert.Equal($"{format} is not a supported injection format", ex.Message);
        }

        [Fact]
        public void Tracer_ExtractAndInject_ShouldUseTheCorrectCodec()
        {
            var carrier = "carrier, yo";
            var spanContext = Substitute.For<ILetsTraceSpanContext>();

            _mockExtractor.Extract(Arg.Is<string>(c => c == carrier));
            _mockInjector.Inject(Arg.Is<ISpanContext>(sc => sc == spanContext), Arg.Is<string>(c => c == carrier));

            _builtTracer.Extract(_format, carrier);
            _builtTracer.Inject(spanContext, _format, carrier);

            _mockExtractor.Received(1).Extract(Arg.Any<string>());
            _mockInjector.Received(1).Inject(Arg.Any<ISpanContext>(), Arg.Any<string>());
        }

        [Fact]
        public void Tracer_SetBaggageItem_ShouldAddAndSetBaggageItems()
        {
            var spanContext = new SpanContext(new TraceId());

            var span = new Span(_builtTracer, "testing", spanContext);

            var key = "key1";
            var value = "value1";
            var value2 = "value2";

            _builtTracer.SetBaggageItem(span, key, value);
            Assert.Equal(value, span.GetBaggageItem(key));

            _builtTracer.SetBaggageItem(span, key, value2);
            Assert.Equal(value2, span.GetBaggageItem(key));
        }

        [Fact]
        public void Tracer_ShouldHaveVarsSetFromBuilder()
        {
            Assert.Equal(_ip, _builtTracer.HostIPv4);
            Assert.Equal(_mockScopeManager, _builtTracer.ScopeManager);
            Assert.Equal(_serviceName, _builtTracer.ServiceName);
            Assert.Equal(_ip, _builtTracer.Tags[Constants.TRACER_IP_TAG_KEY].StringValue);
        }

        [Fact]
        public void Tracer_ActiveSpan_ShouldPullFromTheScopeManager()
        {
            var span = Substitute.For<ISpan>();
            _mockScopeManager.Active.Span.Returns(span);

            Assert.Equal(span, _builtTracer.ActiveSpan);
        }

        [Fact]
        public void Tracer_ActiveSpan_ShouldReturnNullWhenTheActiveSpanIsNull()
        {
            _mockScopeManager.Active.ReturnsNull();

            Assert.Null(_builtTracer.ActiveSpan);
        }

        [Fact]
        public void Tracer_Dispose_ShouldDisposeCorrectly()
        {
            _builtTracer.Dispose();

            _mockSampler.Received(1).Dispose();
            _mockReporter.Received(1).Dispose();
        }
    }
}
