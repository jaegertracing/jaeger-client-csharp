using System;
using System.Collections;
using System.Collections.Generic;
using Jaeger.Core.Propagation;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using OpenTracing;
using OpenTracing.Propagation;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class TracerResiliencyTest
    {
        [Fact]
        public void ShouldFallbackWhenExtractingWithFaultyExtractor()
        {
            Tracer tracer = new Tracer.Builder("TracerResiliencyTestService")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .RegisterExtractor(BuiltinFormats.TextMap, new FaultyExtractor())
                .Build();

            ISpanContext spanContext = tracer.Extract(BuiltinFormats.TextMap, new NoopTextMap());
            Assert.Null(spanContext);
        }

        [Fact]
        public void ShouldFallbackWhenExtractingWithFaultyCodec()
        {
            Tracer tracer = new Tracer.Builder("TracerResiliencyTestService")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .RegisterCodec(BuiltinFormats.TextMap, new FaultyCodec())
                .Build();

            ISpanContext spanContext = tracer.Extract(BuiltinFormats.TextMap, new NoopTextMap());
            Assert.Null(spanContext);
        }

        [Fact]
        public void ShouldFallbackWhenInjectingWithFaultyInjector()
        {
            Tracer tracer = new Tracer.Builder("TracerResiliencyTestService")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .RegisterInjector(BuiltinFormats.TextMap, new FaultyInjector())
                .Build();

            ISpanContext context = tracer.BuildSpan("test-span").Start().Context;
            tracer.Inject(context, BuiltinFormats.TextMap, new NoopTextMap());
        }

        [Fact]
        public void ShouldFallbackWhenInjectingWithFaultyCodec()
        {
            Tracer tracer = new Tracer.Builder("TracerResiliencyTestService")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .RegisterCodec(BuiltinFormats.TextMap, new FaultyCodec())
                .Build();

            ISpanContext context = tracer.BuildSpan("test-span").Start().Context;
            tracer.Inject(context, BuiltinFormats.TextMap, new NoopTextMap());
        }

        private sealed class FaultyInjector : Injector<ITextMap>
        {
            protected override void Inject(SpanContext spanContext, ITextMap carrier)
            {
                throw new InvalidOperationException("Some Codecs can be faulty, this one is.");
            }
        }

        private sealed class FaultyExtractor : Extractor<ITextMap>
        {
            protected override SpanContext Extract(ITextMap carrier)
            {
                throw new InvalidOperationException("Some Codecs can be faulty, this one is.");
            }
        }

        private sealed class FaultyCodec : Codec<ITextMap>
        {
            protected override SpanContext Extract(ITextMap carrier)
            {
                throw new InvalidOperationException("Some Codecs can be faulty, this one is.");
            }

            protected override void Inject(SpanContext spanContext, ITextMap carrier)
            {
                throw new InvalidOperationException("Some Codecs can be faulty, this one is.");
            }
        }

        private sealed class NoopTextMap : ITextMap
        {
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return null;
            }

            public void Set(string key, string value)
            {
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}