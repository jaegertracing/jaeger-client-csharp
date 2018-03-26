using System;
using System.Collections.Generic;
using LetsTrace.Metrics;
using LetsTrace.Samplers;
using NSubstitute;
using OpenTracing;
using Xunit;

namespace LetsTrace.Tests
{
    public class SpanBuilderTests
    {
        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfTracerIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SpanBuilder(null, null, null, null));
            Assert.Equal("tracer", ex.ParamName);
        }

        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfOperationNameIsNull()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();

            var ex = Assert.Throws<ArgumentNullException>(() => new SpanBuilder(tracer, null, null, null));
            Assert.Equal("operationName", ex.ParamName);
        }

        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfSamplerIsNull()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();

            var ex = Assert.Throws<ArgumentNullException>(() => new SpanBuilder(tracer, "op", null, null));
            Assert.Equal("sampler", ex.ParamName);
        }

        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfMetricsIsNull()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var sampler = Substitute.For<ISampler>();

            var ex = Assert.Throws<ArgumentNullException>(() => new SpanBuilder(tracer, "op", sampler, null));
            Assert.Equal("metrics", ex.ParamName);
        }

        [Fact]
        public void SpanBuilder_AddReference_ShouldAddReference()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            var refContext = Substitute.For<ILetsTraceSpanContext>();
            var expectedReferences = new List<Reference> {
                new Reference(References.FollowsFrom, refContext)
            };

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.AddReference(expectedReferences[0].Type, expectedReferences[0].Context);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            Assert.Equal(expectedReferences, builtSpan.References);
        }

        [Fact]
        public void SpanBuilder_AddReference_ShouldAddReference_AndProperlyCalculateParent()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();

            var traceId = new TraceId(4867928, 543789);
            var parentSpanId = new SpanId(139546);
            var baggage = new Dictionary<string, string> { { "key", "value" } };

            var refContext = new SpanContext(traceId, parentSpanId, null, baggage);

            var expectedReferences = new List<Reference> {
                new Reference(References.ChildOf, refContext)
            };

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.AddReference(expectedReferences[0].Type, expectedReferences[0].Context);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            var builtContext = (ILetsTraceSpanContext)builtSpan.Context;

            Assert.Equal(expectedReferences, builtSpan.References);
            Assert.Equal(traceId.ToString(), builtContext.TraceId.ToString());
            Assert.Equal(parentSpanId.ToString(), builtContext.ParentId.ToString());
            Assert.Equal(baggage, builtContext.GetBaggageItems());
        }

        [Fact]
        public void SpanBuilder_WithStartTimestamp_ShouldSetASpecificTimestamp()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));
            var timestamp = DateTimeOffset.Parse("2/12/2018 5:49:19 PM +00:00");

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.WithStartTimestamp(timestamp);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            Assert.Equal(timestamp, builtSpan.StartTimestamp);
        }

        [Fact]
        public void SpanBuilder_WithTag_ShouldSetTags()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            var expectedTags = new Dictionary<string, object> {
                { "boolkey", true },
                { "doublekey", 3D },
                { "intkey", 2 },
                { "stringkey", "string, yo" }
            };

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.WithTag("boolkey", (bool)expectedTags["boolkey"]);
            sb.WithTag("doublekey", (double)expectedTags["doublekey"]);
            sb.WithTag("intkey", (int)expectedTags["intkey"]);
            sb.WithTag("stringkey", (string)expectedTags["stringkey"]);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            Assert.True(builtSpan.Tags["boolkey"] is Field<bool>);
            Assert.Equal(expectedTags["boolkey"], builtSpan.Tags["boolkey"].ValueAs<bool>());
            Assert.True(builtSpan.Tags["doublekey"] is Field<double>);
            Assert.Equal(expectedTags["doublekey"], builtSpan.Tags["doublekey"].ValueAs<double>());
            Assert.True(builtSpan.Tags["intkey"] is Field<int>);
            Assert.Equal(expectedTags["intkey"], builtSpan.Tags["intkey"].ValueAs<int>());
            Assert.True(builtSpan.Tags["stringkey"] is Field<string>);
            Assert.Equal(expectedTags["stringkey"], builtSpan.Tags["stringkey"].ValueAs<string>());
        }

        [Fact]
        public void SpanBuilder_AsChildOf_UsingSpan_ShouldAddReference()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            var refContext = Substitute.For<ILetsTraceSpanContext>();
            var expectedReferences = new List<Reference> {
                new Reference(References.ChildOf, refContext)
            };
            var parentSpan = new Span(tracer, operationName, refContext);

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.AsChildOf(parentSpan);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            Assert.Equal(expectedReferences, builtSpan.References);
        }

        [Fact]
        public void SpanBuilder_AsChildOf_UsingSpanContext_ShouldAddReference()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            var refContext = Substitute.For<ILetsTraceSpanContext>();
            var expectedReferences = new List<Reference> {
                new Reference(References.ChildOf, refContext)
            };

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.AsChildOf(refContext);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            Assert.Equal(expectedReferences, builtSpan.References);
        }

        [Fact]
        public void SpanBuilder_ShouldUseFlagsFromParent()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            ContextFlags flags = (ContextFlags)2;

            var traceId = new TraceId(4867928, 543789);
            var parentSpanId = new SpanId(139546);
            var baggage = new Dictionary<string, string> { { "key", "value" } };

            var refContext = new SpanContext(traceId, parentSpanId, null, baggage, flags);

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.AsChildOf(refContext);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            var context = (ILetsTraceSpanContext)builtSpan.Context;
            Assert.Equal(flags, context.Flags);
        }

        [Fact]
        public void SpanBuilder_ShouldBuildFlagsAndTagsFromSampler()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            var samplerTags = new Dictionary<string, Field> {
                { "tag.1", new Field<string> { Value = "value1" } },
                { "tag.2", new Field<string> { Value = "value2" } }
            };
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>())
                .Returns((true, samplerTags));

            var refContext = Substitute.For<ILetsTraceSpanContext>();

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            var context = (ILetsTraceSpanContext)builtSpan.Context;
            Assert.Equal(ContextFlags.Sampled, context.Flags);
            Assert.Equal(samplerTags, builtSpan.Tags);
        }

        [Fact]
        public void SpanBuilder_ShouldDefaultFlagsToZeroWhenNotSampled()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            var refContext = Substitute.For<ILetsTraceSpanContext>();

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            var builtSpan = (ILetsTraceSpan)sb.Start();

            var context = (ILetsTraceSpanContext)builtSpan.Context;
            Assert.Equal(ContextFlags.None, context.Flags);
        }

        [Fact]
        public void SpanBuilder_StartActive_AddsStartedSpanToScopeManager()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            var scopeManager = Substitute.For<IScopeManager>();
            var operationName = "testing";
            IScope nullScope = null;
            tracer.ScopeManager.Returns(scopeManager);

            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            scopeManager.Active.Returns(nullScope);
            scopeManager.Activate(
                Arg.Is<ISpan>(s => ((ILetsTraceSpan)s).OperationName == operationName),
                Arg.Is<bool>(fsod => fsod == false)
            );

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            var scope = sb.StartActive(false);

            scopeManager.Received(1).Activate(
                Arg.Any<ISpan>(),
                Arg.Any<bool>()
            );
        }

        [Fact]
        public void SpanBuilder_Start_ShouldAddActiveAsParent()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            var scopeManager = Substitute.For<IScopeManager>();
            var operationName = "testing";
            var activeScope = Substitute.For<IScope>();
            var activeSpan = Substitute.For<ISpan>();
            var activeTraceId = new TraceId(34967, 31298);
            var activeSpanId = new SpanId(3829);
            var activeContext = new SpanContext(activeTraceId, activeSpanId);

            activeSpan.Context.Returns(activeContext);
            activeScope.Span.Returns(activeSpan);
            scopeManager.Active.Returns(activeScope);
            tracer.ScopeManager.Returns(scopeManager);
            tracer.ActiveSpan.Returns(activeSpan);

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            var newSpan = sb.Start();

            var newContext = (ILetsTraceSpanContext)newSpan.Context;
            Assert.Equal(activeSpanId, newContext.ParentId);
            Assert.Equal(activeTraceId.Low, newContext.TraceId.Low);
            Assert.Equal(activeTraceId.High, newContext.TraceId.High);
        }

        [Fact]
        public void SpanBuilder_Start_ShouldNotAddActiveAsParent_WhenIgnoreActiveSpanIsSet()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            var scopeManager = Substitute.For<IScopeManager>();
            var operationName = "testing";
            var activeScope = Substitute.For<IScope>();
            var activeSpan = Substitute.For<ISpan>();
            var activeContext = Substitute.For<ILetsTraceSpanContext>();
            var activeTraceId = new TraceId(34967, 31298);
            var activeSpanId = new SpanId(3829);

            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            activeContext.TraceId.Returns(activeTraceId);
            activeContext.SpanId.Returns(activeSpanId);
            activeSpan.Context.Returns(activeContext);
            activeScope.Span.Returns(activeSpan);
            scopeManager.Active.Returns(activeScope);
            tracer.ScopeManager.Returns(scopeManager);
            tracer.ActiveSpan.Returns(activeSpan);

            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.IgnoreActiveSpan();
            var newSpan = sb.Start();

            var newContext = (ILetsTraceSpanContext)newSpan.Context;
            Assert.Equal(0.ToString(), newContext.ParentId.ToString());
            Assert.NotEqual(activeTraceId.Low, newContext.TraceId.Low);
            Assert.NotEqual(activeTraceId.High, newContext.TraceId.High);
        }

        [Fact]
        public void SpanBuilder_Start_ShouldNotAddActiveAsParent_WhenOtherReferencesExist()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var sampler = Substitute.For<ISampler>();
            var metrics = Substitute.For<IMetrics>();
            var scopeManager = Substitute.For<IScopeManager>();
            var operationName = "testing";
            var activeScope = Substitute.For<IScope>();
            var activeSpan = Substitute.For<ISpan>();
            var activeContext = Substitute.For<ILetsTraceSpanContext>();
            var activeTraceId = new TraceId(34967, 31298);
            var activeSpanId = new SpanId(3829);

            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>()).Returns((false, new Dictionary<string, Field>()));

            activeContext.TraceId.Returns(activeTraceId);
            activeContext.SpanId.Returns(activeSpanId);
            activeSpan.Context.Returns(activeContext);
            activeScope.Span.Returns(activeSpan);
            scopeManager.Active.Returns(activeScope);
            tracer.ScopeManager.Returns(scopeManager);
            tracer.ActiveSpan.Returns(activeSpan);

            var refContext = Substitute.For<ILetsTraceSpanContext>();
            var sb = new SpanBuilder(tracer, operationName, sampler, metrics);
            sb.AddReference(References.FollowsFrom, refContext);
            var newSpan = sb.Start();

            var newContext = (ILetsTraceSpanContext)newSpan.Context;
            Assert.Equal(0.ToString(), newContext.ParentId.ToString());
            Assert.NotEqual(activeTraceId.Low, newContext.TraceId.Low);
            Assert.NotEqual(activeTraceId.High, newContext.TraceId.High);
        }
    }
}
