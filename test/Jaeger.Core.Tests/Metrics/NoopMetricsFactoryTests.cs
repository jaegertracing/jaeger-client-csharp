using Jaeger.Core.Metrics;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using System.Collections.Generic;
using Xunit;

namespace Jaeger.Core.Tests.Metrics
{
    /// <summary>
    /// This test is to ensure we have a NOOP implementation that won't crash when used for real.
    /// </summary>
    public class NoopMetricsFactoryTests
    {
        [Fact]
        public void MetricNameIsUsedForCounter()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            metricsFactory.CreateCounter("thecounter", tags);
        }

        [Fact]
        public void CounterValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            ICounter counter = metricsFactory.CreateCounter("thecounter", tags);
            counter.Inc(1);
        }

        [Fact]
        public void MetricNameIsUsedForTimer()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            metricsFactory.CreateTimer("thetimer", tags);
        }

        [Fact]
        public void TimerValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            ITimer timer = metricsFactory.CreateTimer("thetimer", tags);
            timer.DurationTicks(1);
        }

        [Fact]
        public void MetricNameIsUsedForGauge()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            metricsFactory.CreateGauge("thegauge", tags);
        }

        [Fact]
        public void GaugeValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            IGauge gauge = metricsFactory.CreateGauge("thegauge", tags);
            gauge.Update(1);
        }

        [Fact]
        public void CanBeUsedWithMetrics()
        {
            NoopMetricsFactory metricsFactory = NoopMetricsFactory.Instance;
            Tracer tracer =
                new Tracer.Builder("metricsFactoryTest")
                    .WithReporter(new NoopReporter())
                    .WithSampler(new ConstSampler(true))
                    .WithMetrics(new MetricsImpl(metricsFactory))
                    .Build();

            tracer.BuildSpan("theoperation").Start();
        }
    }
}
