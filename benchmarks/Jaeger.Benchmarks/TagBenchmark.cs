using BenchmarkDotNet.Attributes;
using Jaeger.Core;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;

namespace Jaeger.Benchmarks
{
    [MemoryDiagnoser]
    public class TagBenchmark
    {
        private readonly Tracer _tracer;

        public TagBenchmark()
        {
            _tracer = new Tracer.Builder("service")
                .WithReporter(new NoopReporter())
                .WithSampler(new ConstSampler(sample: true))
                .Build();
        }

        [Benchmark]
        public void NoTag()
        {
            _tracer.BuildSpan("foo").Start();
        }

        [Benchmark]
        public void OneTag_on_SpanBuilder()
        {
            _tracer.BuildSpan("foo")
                .WithTag("key1", "value1")
                .Start();
        }

        [Benchmark]
        public void TwoTags_on_SpanBuilder()
        {
            _tracer.BuildSpan("foo")
                .WithTag("key1", "value1")
                .WithTag("key2", "value2")
                .Start();
        }

        [Benchmark]
        public void OneTag_on_Span()
        {
            _tracer.BuildSpan("foo")
                .Start()
                .SetTag("key1", "value1");
        }

        [Benchmark]
        public void TwoTags_on_Span()
        {
            _tracer.BuildSpan("foo")
                .Start()
                .SetTag("key1", "value1")
                .SetTag("key2", "value2");
        }

        [Benchmark]
        public void OneTag_on_SpanBuilder_OneTag_on_Span()
        {
            _tracer.BuildSpan("foo")
                .WithTag("key1", "value1")
                .Start()
                .SetTag("key2", "value2");
        }
    }
}