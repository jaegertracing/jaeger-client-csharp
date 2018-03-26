using BenchmarkDotNet.Attributes;
using OpenTracing;

namespace LetsTrace.Benchmarks
{
    [MemoryDiagnoser]
    public class SpanBuilderBenchmark
    {
        private readonly Tracer _tracer;
        private readonly ISpan _ref1;
        private readonly ISpan _ref2;

        public SpanBuilderBenchmark()
        {
            _tracer = new Tracer.Builder("service").Build();

            _ref1 = _tracer.BuildSpan("ref1").Start();
            _ref2 = _tracer.BuildSpan("ref2").Start();
        }

        [Benchmark]
        public void Start_No_Reference()
        {
            _tracer.BuildSpan("foo").Start();
        }

        [Benchmark]
        public void Start_One_Reference()
        {
            _tracer.BuildSpan("foo")
                .AsChildOf(_ref1)
                .Start();
        }

        [Benchmark]
        public void Start_Two_References()
        {
            _tracer.BuildSpan("foo")
                .AsChildOf(_ref1)
                .AsChildOf(_ref2)
                .Start();
        }
    }
}