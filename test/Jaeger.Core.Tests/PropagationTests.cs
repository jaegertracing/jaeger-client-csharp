using System.Collections.Generic;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class PropagationTest
    {
        [Fact]
        public void TestDebugCorrelationId()
        {
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .Build();

            var headers = new Dictionary<string, string>();
            headers.Add(Constants.DebugIdHeaderKey, "Coraline");
            ITextMap carrier = new TextMapExtractAdapter(headers);
            SpanContext spanContext = (SpanContext)tracer.Extract(BuiltinFormats.TextMap, carrier);
            Assert.True(spanContext.IsDebugIdContainerOnly());
            Assert.Equal("Coraline", spanContext.DebugId);
            Span span = (Span)tracer.BuildSpan("span").AsChildOf(spanContext).Start();
            spanContext = (SpanContext)span.Context;
            Assert.True(spanContext.IsSampled);
            Assert.True(spanContext.IsDebug);
            Assert.Equal("Coraline", span.GetTags()[Constants.DebugIdHeaderKey]);
        }

        [Fact]
        public void TestActiveSpanPropagation()
        {
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .Build();

            using (IScope parent = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                Assert.Equal(parent, tracer.ScopeManager.Active);
            }
        }

        [Fact]
        public void TestActiveSpanAutoReference()
        {
            InMemoryReporter reporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            using (IScope parent = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                tracer.BuildSpan("child").StartActive(finishSpanOnDispose: true).Dispose();
            }
            Assert.Equal(2, reporter.GetSpans().Count);

            Span childSpan = reporter.GetSpans()[0];
            Span parentSpan = reporter.GetSpans()[1];

            Assert.Equal("child", childSpan.OperationName);
            Assert.Single(childSpan.GetReferences());
            Assert.Equal("parent", parentSpan.OperationName);
            Assert.Empty(parentSpan.GetReferences());
            Assert.Equal(References.ChildOf, childSpan.GetReferences()[0].Type);
            Assert.Equal(parentSpan.Context, childSpan.GetReferences()[0].Context);
            Assert.Equal(parentSpan.Context.TraceId, childSpan.Context.TraceId);
            Assert.Equal(parentSpan.Context.SpanId, childSpan.Context.ParentId);
        }

        [Fact]
        public void TestActiveSpanAutoFinishOnClose()
        {
            InMemoryReporter reporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true).Dispose();
            Assert.Single(reporter.GetSpans());
        }

        [Fact]
        public void TestActiveSpanNotAutoFinishOnClose()
        {
            InMemoryReporter reporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            IScope scope = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: false);
            Span span = (Span)scope.Span;
            scope.Dispose();
            Assert.Empty(reporter.GetSpans());
            span.Finish();
            Assert.Single(reporter.GetSpans());
        }

        [Fact]
        public void TestIgnoreActiveSpan()
        {
            InMemoryReporter reporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            using (IScope parent = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                tracer.BuildSpan("child").IgnoreActiveSpan().StartActive(finishSpanOnDispose: true).Dispose();
            }
            Assert.Equal(2, reporter.GetSpans().Count);

            Span childSpan = reporter.GetSpans()[0];
            Span parentSpan = reporter.GetSpans()[1];

            Assert.Empty(reporter.GetSpans()[0].GetReferences());
            Assert.Empty(reporter.GetSpans()[1].GetReferences());
            Assert.NotEqual(parentSpan.Context.TraceId, childSpan.Context.TraceId);
            Assert.Equal(0L, childSpan.Context.ParentId);
        }

        [Fact]
        public void TestNoAutoRefWithExistingRefs()
        {
            InMemoryReporter reporter = new InMemoryReporter();
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            ISpan initialSpan = tracer.BuildSpan("initial").Start();

            using (IScope parent = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                tracer.BuildSpan("child").AsChildOf(initialSpan.Context).StartActive(finishSpanOnDispose: true).Dispose();
            }

            initialSpan.Finish();

            Assert.Equal(3, reporter.GetSpans().Count);

            Span childSpan = reporter.GetSpans()[0];
            Span parentSpan = reporter.GetSpans()[1];
            Span initSpan = reporter.GetSpans()[2];

            Assert.Empty(initSpan.GetReferences());
            Assert.Empty(parentSpan.GetReferences());

            Assert.Equal(initSpan.Context.TraceId, childSpan.Context.TraceId);
            Assert.Equal(initSpan.Context.SpanId, childSpan.Context.ParentId);

            Assert.Equal(0L, initSpan.Context.ParentId);
            Assert.Equal(0L, parentSpan.Context.ParentId);
        }

        [Fact]
        public void TestCustomScopeManager()
        {
            IScope scope = Substitute.For<IScope>();
            Tracer tracer = new Tracer.Builder("test")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .WithScopeManager(new CustomScopeManager(scope))
                .Build();

            Assert.Equal(scope, tracer.ScopeManager.Active);
        }

        private class CustomScopeManager : IScopeManager
        {
            private readonly IScope _scope;

            public IScope Active => _scope;

            public CustomScopeManager(IScope scope)
            {
                _scope = scope;
            }

            public IScope Activate(ISpan span, bool finishSpanOnDispose) => _scope;
        }
    }
}