using System.Collections.Generic;
using Jaeger.Core.Baggage;
using Jaeger.Core.Metrics;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Baggage
{
    public class BaggageSetterTests
    {
        private InMemoryReporter _reporter;
        private Tracer _tracer;
        private Span _span;
        private InMemoryMetricsFactory _metricsFactory;
        private IMetrics _metrics;
        private IBaggageRestrictionManager _mgr;
        private BaggageSetter _setter;

        private static readonly string Key = "key";
        private static readonly string Service = "SamplerTest";

        public BaggageSetterTests()
        {
            _metricsFactory = new InMemoryMetricsFactory();
            _reporter = new InMemoryReporter();
            _metrics = new MetricsImpl(_metricsFactory);
            _mgr = Substitute.For<IBaggageRestrictionManager>();
            _setter = new BaggageSetter(_mgr, _metrics);

            _tracer = new Tracer.Builder(Service)
                .WithReporter(_reporter)
                .WithSampler(new ConstSampler(true))
                .WithMetrics(_metrics)
                .Build();

            _span = (Span)_tracer.BuildSpan("some-operation").Start();
        }

        [Fact]
        public void TestInvalidBaggage()
        {
            _mgr.GetRestriction(Service, Key).Returns(new Restriction(false, 0));

            string value = "value";
            SpanContext ctx = _setter.SetBaggage(_span, Key, value);

            AssertBaggageLogs(_span, Key, value, false, false, true);
            Assert.Null(ctx.GetBaggageItem(Key));

            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:baggage_updates", "result=err"));
        }

        [Fact]
        public void TestTruncatedBaggage()
        {
            _mgr.GetRestriction(Service, Key).Returns(new Restriction(true, 5));
            string value = "0123456789";
            string expected = "01234";
            SpanContext ctx = _setter.SetBaggage(_span, Key, value);

            AssertBaggageLogs(_span, Key, expected, true, false, false);
            Assert.Equal(expected, ctx.GetBaggageItem(Key));

            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:baggage_truncations", ""));
            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:baggage_updates", "result=ok"));
        }

        [Fact]
        public void TestOverrideBaggage()
        {
            _mgr.GetRestriction(Service, Key).Returns(new Restriction(true, 5));
            string value = "value";
            SpanContext ctx = _setter.SetBaggage(_span, Key, value);
            Span child = (Span)_tracer.BuildSpan("some-operation").AsChildOf(ctx).Start();
            ctx = _setter.SetBaggage(child, Key, value);

            AssertBaggageLogs(child, Key, value, false, true, false);
            Assert.Equal(value, ctx.GetBaggageItem(Key));

            Assert.Equal(2, _metricsFactory.GetCounter("jaeger:baggage_updates", "result=ok"));
        }

        [Fact]
        public void TestUnsampledSpan()
        {
            _tracer = new Tracer.Builder("SamplerTest")
                .WithReporter(_reporter)
                .WithSampler(new ConstSampler(false))
                .WithMetrics(_metrics)
                .Build();

            _span = (Span)_tracer.BuildSpan("some-operation").Start();

            _mgr.GetRestriction(Service, Key).Returns(new Restriction(true, 5));
            string value = "value";
            SpanContext ctx = _setter.SetBaggage(_span, Key, value);

            Assert.Equal(value, ctx.GetBaggageItem(Key));
            // No logs should be written if the span is not sampled
            Assert.Empty(_span.GetLogs());
        }

        [Fact]
        public void TestBaggageNullValueTolerated()
        {
            _mgr.GetRestriction(Service, Key).Returns(new Restriction(true, 5));
            string value = null;
            SpanContext ctx = _setter.SetBaggage(_span, Key, value);

            AssertBaggageLogs(_span, Key, null, false, false, false);
            Assert.Null(ctx.GetBaggageItem(Key));
        }

        [Fact]
        public void TestBaggageNullRemoveValue()
        {
            _mgr.GetRestriction(Service, Key).Returns(new Restriction(true, 5));
            string value = "value";
            Span originalSpan = (Span)_span.SetBaggageItem(Key, value);
            Assert.Equal(value, originalSpan.GetBaggageItem(Key));
            Span child = (Span)_tracer.BuildSpan("some-operation").AsChildOf(originalSpan).Start();
            child = (Span)child.SetBaggageItem(Key, null);

            AssertBaggageLogs(child, Key, null, false, true, false);
            Assert.Null(child.GetBaggageItem(Key));

            Assert.Equal(2, _metricsFactory.GetCounter("jaeger:baggage_updates", "result=ok"));
        }

        private void AssertBaggageLogs(
            Span span,
            string key,
            string value,
            bool truncate,
            bool @override,
            bool invalid
        )
        {
            var logs = span.GetLogs();
            Assert.NotEmpty(logs);
            IDictionary<string, object> fields = logs[logs.Count - 1].Fields;
            Assert.Equal("baggage", fields["event"]);
            Assert.Equal(key, fields["key"]);
            Assert.Equal(value, fields["value"]);
            if (truncate)
            {
                Assert.True((bool)fields["truncated"]);
            }
            if (@override)
            {
                Assert.True((bool)fields["override"]);
            }
            if (invalid)
            {
                Assert.True((bool)fields["invalid"]);
            }
        }
    }
}
