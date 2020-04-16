using System;
using System.Collections.Generic;
using Jaeger.Crossdock.Model;
using Jaeger.Metrics;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using OpenTracing;

namespace Jaeger.Crossdock.Behavior
{
    public class EndToEndBehavior
    {
        private readonly IDictionary<string, ITracer> _tracers;

        public EndToEndBehavior(string samplingHostPort, string serviceName, ISender sender)
        {
            IMetrics metrics = new MetricsImpl(NoopMetricsFactory.Instance);
            IReporter reporter = new RemoteReporter.Builder()
                .WithSender(sender)
                .WithFlushInterval(TimeSpan.FromSeconds(1))
                .WithMaxQueueSize(100)
                .WithMetrics(metrics)
                .Build();

            var constSampler = new ConstSampler(true);

            _tracers = new Dictionary<string, ITracer>
            {
                {RemoteControlledSampler.Type, GetRemoteTracer(metrics, reporter, serviceName, samplingHostPort)},
                {ConstSampler.Type, new Tracer.Builder(serviceName).WithReporter(reporter).WithSampler(constSampler).Build()}
            };
        }

        private Tracer GetRemoteTracer(IMetrics metrics, IReporter reporter, string serviceName, string samplingHostPort)
        {
            ISampler initialSampler = new ProbabilisticSampler(1.0);
            var manager = new HttpSamplingManager(samplingHostPort);

            var remoteSampler = new RemoteControlledSampler.Builder(serviceName)
                .WithSamplingManager(manager)
                .WithInitialSampler(initialSampler)
                .WithMetrics(metrics)
                .WithPollingInterval(TimeSpan.FromSeconds(5))
                .Build();

            return new Tracer.Builder(serviceName)
                .WithReporter(reporter)
                .WithSampler(remoteSampler)
                .Build();
        }

        public void GenerateTraces(CreateTracesRequest request)
        {
            var samplerType = request.Type;
            var tracer = _tracers[samplerType];
            for (var i = 0; i < request.Count; i++)
            {
                var builder = tracer.BuildSpan(request.Operation);
                if (request.Tags != null)
                {
                    foreach (var kv in request.Tags)
                    {
                        builder.WithTag(kv.Key, kv.Value);
                    }
                }

                var span = builder.Start();
                span.Finish();
            }
        }
    }
}
