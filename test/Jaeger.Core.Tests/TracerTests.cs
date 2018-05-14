using System;
using System.Threading;
using Jaeger.Core.Metrics;
using Jaeger.Core.Propagation;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class TracerTests
    {
        private Tracer _tracer;
        private readonly InMemoryMetricsFactory _metricsFactory;

        public TracerTests()
        {
            _metricsFactory = new InMemoryMetricsFactory();

            _tracer = new Tracer.Builder("TracerTestService")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .WithMetrics(new MetricsImpl(_metricsFactory))
                .Build();
        }

        [Fact]
        public void TestDefaultConstructor()
        {
            var tracer = new Tracer.Builder("name").Build();
            Assert.IsType<RemoteReporter>(tracer.Reporter);
            // no exception
            tracer.BuildSpan("foo").Start().Finish();
        }

        [Fact]
        public void TestBuildSpan()
        {
            string expectedOperation = "fry";
            Span span = (Span)_tracer.BuildSpan(expectedOperation).Start();

            Assert.Equal(expectedOperation, span.OperationName);
        }

        [Fact]
        public void TestTracerMetrics()
        {
            string expectedOperation = "fry";
            _tracer.BuildSpan(expectedOperation).Start();
            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:started_spans", "sampled=y"));
            Assert.Equal(0, _metricsFactory.GetCounter("jaeger:started_spans", "sampled=n"));
            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:traces", "sampled=y,state=started"));
            Assert.Equal(0, _metricsFactory.GetCounter("jaeger:traces", "sampled=n,state=started"));
        }

        [Fact]
        public void TestRegisterInjector()
        {
            Injector<ITextMap> injector = Substitute.For<Injector<ITextMap>>();

            _tracer = new Tracer.Builder("TracerTestService")
                    .WithReporter(new InMemoryReporter())
                    .WithSampler(new ConstSampler(true))
                    .WithMetrics(new MetricsImpl(new InMemoryMetricsFactory()))
                    .RegisterInjector(BuiltinFormats.TextMap, injector)
                    .Build();

            Span span = (Span)_tracer.BuildSpan("leela").Start();

            ITextMap carrier = Substitute.For<ITextMap>();
            _tracer.Inject(span.Context, BuiltinFormats.TextMap, carrier);

            injector.Received(1).Inject(Arg.Any<SpanContext>(), Arg.Any<ITextMap>());
        }

        [Fact]
        public void TestServiceNameNotNull()
        {
            Assert.Throws<ArgumentException>(() => new Tracer.Builder(null));
        }

        [Fact]
        public void TestServiceNameNotEmptyNull()
        {
            Assert.Throws<ArgumentException>(() => new Tracer.Builder("  "));
        }

        [Fact]
        public void TestBuilderIsServerRpc()
        {
            SpanBuilder spanBuilder = (SpanBuilder)_tracer.BuildSpan("ndnd");
            spanBuilder.WithTag(Tags.SpanKind.Key, "server");

            Assert.True(spanBuilder.IsRpcServer());
        }

        [Fact]
        public void TestBuilderIsNotServerRpc()
        {
            SpanBuilder spanBuilder = (SpanBuilder)_tracer.BuildSpan("Lrrr");
            spanBuilder.WithTag(Tags.SpanKind.Key, "peachy");

            Assert.False(spanBuilder.IsRpcServer());
        }

        [Fact]
        public void TestWithBaggageRestrictionManager()
        {
            _tracer = new Tracer.Builder("TracerTestService")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .WithMetrics(new MetricsImpl(_metricsFactory))
                .Build();
            Span span = (Span)_tracer.BuildSpan("some-operation").Start();
            string key = "key";
            _tracer.SetBaggage(span, key, "value");

            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:baggage_updates", "result=ok"));
        }

        [Fact]
        public void TestTracerImplementsDisposable()
        {
            Assert.IsAssignableFrom<IDisposable>(_tracer);
        }

        [Fact]
        public void TestDispose()
        {
            IReporter reporter = Substitute.For<IReporter>();
            ISampler sampler = Substitute.For<ISampler>();

            var tracer = new Tracer.Builder("bonda")
                .WithReporter(reporter)
                .WithSampler(sampler)
                .Build();

            tracer.Dispose();
            reporter.Received(1).CloseAsync(Arg.Any<CancellationToken>());
            sampler.Received(1).Close();
        }

        [Fact]
        public void TestAsChildOfAcceptNull()
        {
            Span span = (Span)_tracer.BuildSpan("foo").AsChildOf((Span)null).Start();
            span.Finish();
            Assert.Empty(span.GetReferences());

            span = (Span)_tracer.BuildSpan("foo").AsChildOf((ISpanContext)null).Start();
            span.Finish();
            Assert.Empty(span.GetReferences());
        }

        [Fact]
        public void TestActiveSpan()
        {
            var mockSpan = Substitute.For<ISpan>();
            _tracer.ScopeManager.Activate(mockSpan, true);
            Assert.Equal(mockSpan, _tracer.ActiveSpan);
        }

        [Fact]
        public void TestSpanContextNotSampled()
        {
            string expectedOperation = "fry";
            Span first = (Span)_tracer.BuildSpan(expectedOperation).Start();
            _tracer.BuildSpan(expectedOperation).AsChildOf(first.Context.WithFlags((byte)0)).Start();

            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:started_spans", "sampled=y"));
            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:started_spans", "sampled=n"));
            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:traces", "sampled=y,state=started"));
            Assert.Equal(0, _metricsFactory.GetCounter("jaeger:traces", "sampled=n,state=started"));
        }
    }
}
