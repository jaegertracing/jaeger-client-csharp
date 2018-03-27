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
        private Tracer GetTracer()
        {
            return new Tracer.Builder("service").Build();
        }

        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfTracerIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SpanBuilder(null, null, null, null));
            Assert.Equal("tracer", ex.ParamName);
        }

        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfOperationNameIsNull()
        {
            var tracer = GetTracer();

            var ex = Assert.Throws<ArgumentNullException>(() => new SpanBuilder(tracer, null, null, null));
            Assert.Equal("operationName", ex.ParamName);
        }

        [Fact]
        public void SpanBuilder_Constructor_ShouldThrowIfSamplerIsNull()
        {
            var tracer = GetTracer();

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
        public void SpanBuilder_AddReference_IgnoresNullContext()
        {
            var tracer = GetTracer();

            var span = (ILetsTraceSpan)tracer.BuildSpan("testing")
                .AddReference(References.ChildOf, null)
                .Start();

            Assert.Empty(span.References);
        }

        [Fact]
        public void SpanBuilder_AddReference_ShouldAddReference()
        {
            var tracer = GetTracer();

            // Reference
            var referencedSpan = (ILetsTraceSpan)tracer.BuildSpan("parent").Start();
            Assert.Empty(referencedSpan.References);

            // Child
            var span = (ILetsTraceSpan)tracer.BuildSpan("child")
                .AddReference(References.FollowsFrom, referencedSpan.Context)
                .Start();

            Assert.Collection(span.References, item =>
            {
                Assert.Equal(References.FollowsFrom, item.Type);
                Assert.Same(referencedSpan.Context, item.Context);
            });
        }

        [Fact]
        public void SpanBuilder_AddReference_ShouldAddReference_AndProperlyCalculateParent()
        {
            var tracer = GetTracer();

            // Reference
            var referencedSpan = (ILetsTraceSpan)tracer.BuildSpan("parent")
                .Start()
                .SetBaggageItem("key", "value");

            Assert.Empty(referencedSpan.References);

            // Child
            var span = (ILetsTraceSpan)tracer.BuildSpan("child")
                .AddReference(References.ChildOf, referencedSpan.Context)
                .Start();

            var builtContext = (ILetsTraceSpanContext)span.Context;

            Assert.Single(span.References);
            Assert.Equal(referencedSpan.Context.TraceId, span.Context.TraceId);
            Assert.Equal(referencedSpan.Context.SpanId, span.Context.ParentId);
            Assert.Collection(builtContext.GetBaggageItems(), kvp =>
            {
                Assert.Equal("key", kvp.Key);
                Assert.Equal("value", kvp.Value);
            });
        }

        [Fact]
        public void SpanBuilder_WithStartTimestamp_ShouldSetASpecificTimestamp()
        {
            var tracer = GetTracer();

            var timestamp = new DateTimeOffset(2018, 2, 12, 17, 49, 19, TimeSpan.Zero);
            var span = (ILetsTraceSpan)tracer.BuildSpan("foo")
                .WithStartTimestamp(timestamp)
                .Start();

            Assert.Equal(timestamp.UtcDateTime, span.StartTimestampUtc);
        }

        [Fact]
        public void SpanBuilder_WithTag_ShouldSetTags()
        {
            var tracer = GetTracer();

            var builtSpan = (ILetsTraceSpan)tracer.BuildSpan("foo")
                .WithTag("boolkey", true)
                .WithTag("doublekey", 3d)
                .WithTag("intkey", 2)
                .WithTag("stringkey", "string, yo")
                .Start();

            Assert.True((bool)builtSpan.Tags["boolkey"]);
            Assert.Equal(3d, builtSpan.Tags["doublekey"]);
            Assert.Equal(2, builtSpan.Tags["intkey"]);
            Assert.Equal("string, yo", builtSpan.Tags["stringkey"]);
        }

        [Fact]
        public void SpanBuilder_AsChildOf_IgnoresNullSpan()
        {
            var tracer = GetTracer();

            var builtSpan = (ILetsTraceSpan)tracer.BuildSpan("foo")
                .AsChildOf((ISpan)null)
                .Start();

            Assert.Empty(builtSpan.References);
        }

        [Fact]
        public void SpanBuilder_AsChildOf_IgnoresNullContext()
        {
            var tracer = GetTracer();

            var builtSpan = (ILetsTraceSpan)tracer.BuildSpan("foo")
                .AsChildOf((ISpanContext)null)
                .Start();

            Assert.Empty(builtSpan.References);
        }

        [Fact]
        public void SpanBuilder_AsChildOf_UsingSpan_ShouldAddReference()
        {
            var tracer = GetTracer();

            // Reference
            var parentSpan = (ILetsTraceSpan)tracer.BuildSpan("parent").Start();
            Assert.Empty(parentSpan.References);

            // Child
            var span = (ILetsTraceSpan)tracer.BuildSpan("child")
                .AsChildOf(parentSpan)
                .Start();

            Assert.Collection(span.References, item =>
            {
                Assert.Equal(References.ChildOf, item.Type);
                Assert.Same(parentSpan.Context, item.Context);
            });
        }

        [Fact]
        public void SpanBuilder_AsChildOf_UsingSpanContext_ShouldAddReference()
        {
            var tracer = GetTracer();

            // Reference
            var parentSpan = (ILetsTraceSpan)tracer.BuildSpan("parent").Start();
            Assert.Empty(parentSpan.References);

            // Child
            var span = (ILetsTraceSpan)tracer.BuildSpan("child")
                .AsChildOf(parentSpan.Context)
                .Start();

            Assert.Collection(span.References, item =>
            {
                Assert.Equal(References.ChildOf, item.Type);
                Assert.Same(parentSpan.Context, item.Context);
            });
        }

        [Fact]
        public void SpanBuilder_ShouldUseFlagsFromParent()
        {
            var tracer = new Tracer.Builder("service")
                .WithSampler(new ConstSampler(true))
                .Build();

            var parentSpan = (ILetsTraceSpan)tracer.BuildSpan("parent").Start();
            Assert.True(parentSpan.Context.Flags.HasFlag(ContextFlags.Sampled));

            var childSpan = (ILetsTraceSpan)tracer.BuildSpan("child").AsChildOf(parentSpan).Start();
            Assert.Equal(parentSpan.Context.Flags, childSpan.Context.Flags);
        }

        [Fact]
        public void SpanBuilder_ShouldBuildFlagsAndTagsFromSampler()
        {
            var sampler = Substitute.For<ISampler>();
            var samplerTags = new Dictionary<string, object> {
                { "tag.1", "value1" },
                { "tag.2", "value2" }
            };
            sampler.IsSampled(Arg.Any<TraceId>(), Arg.Any<string>())
                .Returns((true, samplerTags));

            var tracer = new Tracer.Builder("service")
                .WithSampler(sampler)
                .Build();

            var builtSpan = (ILetsTraceSpan)tracer.BuildSpan("foo").Start();

            Assert.Equal(ContextFlags.Sampled, builtSpan.Context.Flags);
            Assert.Equal(samplerTags, builtSpan.Tags);
        }

        [Fact]
        public void SpanBuilder_ShouldDefaultFlagsToZeroWhenNotSampled()
        {
            var tracer = new Tracer.Builder("service")
                .WithSampler(new ConstSampler(false))
                .Build();

            var builtSpan = (ILetsTraceSpan)tracer.BuildSpan("foo").Start();

            Assert.Equal(ContextFlags.None, builtSpan.Context.Flags);
        }

        [Fact]
        public void SpanBuilder_StartActive_AddsStartedSpanToScopeManager()
        {
            var tracer = GetTracer();

            using (var scope = tracer.BuildSpan("foo").StartActive(finishSpanOnDispose: true))
            {
                Assert.Equal(scope, tracer.ScopeManager.Active);
                Assert.IsType<Span>(scope.Span);
            }

            Assert.Null(tracer.ScopeManager.Active);
        }

        [Fact]
        public void SpanBuilder_Start_ShouldAddActiveAsParent()
        {
            var tracer = GetTracer();

            using (var parentScope = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                var parentSpan = (ILetsTraceSpan)parentScope.Span;
                var newSpan = (ILetsTraceSpan)tracer.BuildSpan("child").Start();

                Assert.Single(newSpan.References);
                Assert.Equal(parentSpan.Context.TraceId, newSpan.Context.TraceId);
                Assert.Equal(parentSpan.Context.SpanId, newSpan.Context.ParentId);
            }
        }

        [Fact]
        public void SpanBuilder_Start_ShouldNotAddActiveAsParent_WhenIgnoreActiveSpanIsSet()
        {
            var tracer = GetTracer();

            using (var parentScope = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                var parentSpan = (ILetsTraceSpan)parentScope.Span;
                var newSpan = (ILetsTraceSpan)tracer.BuildSpan("child")
                    .IgnoreActiveSpan()
                    .Start();

                Assert.Empty(newSpan.References);
                Assert.Equal(new SpanId(0).ToString(), newSpan.Context.ParentId.ToString()); // TODO SpanId should implement Equals()
                Assert.NotEqual(parentSpan.Context.TraceId, newSpan.Context.TraceId);
            }
        }

        [Fact]
        public void SpanBuilder_Start_ShouldNotAddActiveAsParent_WhenOtherReferencesExist()
        {
            var tracer = GetTracer();

            var otherReference = (ILetsTraceSpan)tracer.BuildSpan("reference").Start();

            using (var parentScope = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
            {
                var newSpan = (ILetsTraceSpan)tracer.BuildSpan("child")
                    .AsChildOf(otherReference)
                    .Start();

                Assert.Collection(newSpan.References, item =>
                {
                    Assert.Equal(References.ChildOf, item.Type);
                    Assert.Same(otherReference.Context, item.Context);
                });

                Assert.Equal(otherReference.Context.TraceId, newSpan.Context.TraceId);
                Assert.Equal(otherReference.Context.SpanId, newSpan.Context.ParentId);
            }
        }
    }
}
