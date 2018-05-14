using System;
using BenchmarkDotNet.Attributes;
using Jaeger.Core;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;

namespace Jaeger.Benchmarks
{
    [MemoryDiagnoser]
    public class DateTimeHandlingBenchmark
    {
        private readonly Tracer _tracer;

        public DateTimeHandlingBenchmark()
        {
            _tracer = new Tracer.Builder("service")
                .WithReporter(new NoopReporter())
                .WithSampler(new ConstSampler(sample: true))
                .Build();
        }

        [Benchmark]
        public void NoTimestamp_Start()
        {
            _tracer.BuildSpan("foo").Start();
        }

        [Benchmark]
        public void NoTimestamp_Start_Finish()
        {
            _tracer.BuildSpan("foo").Start().Finish();
        }

        [Benchmark]
        public void NoTimestamp_Start_Log_Finish()
        {
            _tracer.BuildSpan("foo").Start().Log("bar").Finish();
        }

        [Benchmark]
        public void WithTimestamp_Start()
        {
            _tracer.BuildSpan("foo")
                .WithStartTimestamp(DateTimeOffset.Now)
                .Start();
        }

        [Benchmark]
        public void WithTimestamp_Start_Finish()
        {
            _tracer.BuildSpan("foo")
                .WithStartTimestamp(DateTimeOffset.Now)
                .Start()
                .Finish(DateTimeOffset.Now);
        }

        [Benchmark]
        public void WithTimestamp_Start_Log_Finish()
        {
            _tracer.BuildSpan("foo")
                .WithStartTimestamp(DateTimeOffset.Now)
                .Start()
                .Log(DateTimeOffset.Now, "bar")
                .Finish(DateTimeOffset.Now);
        }
    }
}