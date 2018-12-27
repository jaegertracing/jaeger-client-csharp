using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jaeger;
using Jaeger.Metrics;
using Jaeger.Reporters;
using Jaeger.Senders;
using Microsoft.Extensions.Logging;

namespace Jaeger.Benchmarks
{
    public class RemoteReporterBenchmark
    {
        private const int QueueCount = 10_000_000;
        public async Task RunReport()
        {
            var sender = new MockSender();
            var reporter =  new RemoteReporter.Builder()
                .WithMaxQueueSize(100)
                .WithSender(sender)
                .Build();

            Console.WriteLine($"reporter: {reporter.GetType().Name}");

            var finished = new CountdownEvent(1 + (int)QueueCount);
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < QueueCount; ++i)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        reporter.Report(new MockSpan());
                    }
                    finally
                    {
                        finished.Signal();
                    }
                }, null);
            }

            finished.Signal(); // Signal that queueing is complete.
            finished.Wait();
            await reporter.CloseAsync(default).ConfigureAwait(false);
            sw.Stop();

            Console.WriteLine($"try report {QueueCount} spans");
            Console.WriteLine($"send {sender.Count} spans in {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"throughput: {sender.Count / sw.ElapsedMilliseconds}k/s");
        }
    }

    public class MockSender : ISender
    {
        private long cnt = 0;
        public long Count => cnt;
        public Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref cnt);
            return Task.FromResult(0);
        }

        public Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
    
    public class MockSpan : Jaeger.Span
    {
        internal MockSpan()
            : base(default, default, default, default, new Dictionary<string, object>(), default)
        {
        }
    }

}
