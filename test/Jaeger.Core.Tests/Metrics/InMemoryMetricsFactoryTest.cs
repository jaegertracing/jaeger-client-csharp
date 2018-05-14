using Jaeger.Core.Metrics;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using System.Collections.Generic;
using Xunit;

namespace Jaeger.Core.Tests.Metrics
{
    public class InMemoryMetricsFactoryTest
    {
        [Fact]
        public void MetricNameIsUsedForCounter()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            InMemoryMetricsFactory inMemoryMetricsFactory = new InMemoryMetricsFactory();
            inMemoryMetricsFactory.CreateCounter("thecounter", tags);

            Assert.Equal(-1, inMemoryMetricsFactory.GetCounter("thecounter", new Dictionary<string, string>()));
            Assert.Equal(0, inMemoryMetricsFactory.GetCounter("thecounter", tags));
        }

        [Fact]
        public void CounterValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            InMemoryMetricsFactory inMemoryMetricsFactory = new InMemoryMetricsFactory();
            ICounter counter = inMemoryMetricsFactory.CreateCounter("thecounter", tags);
            Assert.Equal(0, inMemoryMetricsFactory.GetCounter("thecounter", tags));

            counter.Inc(1);

            Assert.Equal(1, inMemoryMetricsFactory.GetCounter("thecounter", tags));
        }

        [Fact]
        public void MetricNameIsUsedForTimer()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            InMemoryMetricsFactory inMemoryMetricsFactory = new InMemoryMetricsFactory();
            inMemoryMetricsFactory.CreateTimer("thetimer", tags);

            Assert.Equal(-1, inMemoryMetricsFactory.GetTimer("thetimer", new Dictionary<string, string>()));
            Assert.Equal(0, inMemoryMetricsFactory.GetTimer("thetimer", tags));
        }

        [Fact]
        public void TimerValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            InMemoryMetricsFactory inMemoryMetricsFactory = new InMemoryMetricsFactory();
            ITimer timer = inMemoryMetricsFactory.CreateTimer("thetimer", tags);
            Assert.Equal(0, inMemoryMetricsFactory.GetTimer("thetimer", tags));

            timer.DurationTicks(1);

            Assert.Equal(1, inMemoryMetricsFactory.GetTimer("thetimer", tags));
        }

        [Fact]
        public void MetricNameIsUsedForGauge()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            InMemoryMetricsFactory inMemoryMetricsFactory = new InMemoryMetricsFactory();
            inMemoryMetricsFactory.CreateGauge("thegauge", tags);

            Assert.Equal(-1, inMemoryMetricsFactory.GetGauge("thegauge", new Dictionary<string, string>()));
            Assert.Equal(0, inMemoryMetricsFactory.GetGauge("thegauge", tags));
        }

        [Fact]
        public void GaugeValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            InMemoryMetricsFactory inMemoryMetricsFactory = new InMemoryMetricsFactory();
            IGauge gauge = inMemoryMetricsFactory.CreateGauge("thegauge", tags);
            Assert.Equal(0, inMemoryMetricsFactory.GetGauge("thegauge", tags));

            gauge.Update(1);

            Assert.Equal(1, inMemoryMetricsFactory.GetGauge("thegauge", tags));
        }

        [Fact]
        public void EmptyValueForTag()
        {
            InMemoryMetricsFactory metricsFactory = new InMemoryMetricsFactory();
            Tracer tracer = new Tracer.Builder("metricsFactoryTest")
                .WithReporter(new NoopReporter())
                .WithSampler(new ConstSampler(true))
                .WithMetrics(new MetricsImpl(metricsFactory))
                .Build();

            tracer.BuildSpan("theoperation").Start();
            Assert.Equal(-1, metricsFactory.GetCounter("jaeger:started_spans", "sampled"));
            Assert.Equal(-1, metricsFactory.GetCounter("jaeger:started_spans", ""));
        }

        [Fact]
        public void CanBeUsedWithMetrics()
        {
            InMemoryMetricsFactory metricsFactory = new InMemoryMetricsFactory();
            Tracer tracer = new Tracer.Builder("metricsFactoryTest")
                .WithReporter(new NoopReporter())
                .WithSampler(new ConstSampler(true))
                .WithMetrics(new MetricsImpl(metricsFactory))
                .Build();

            tracer.BuildSpan("theoperation").Start();
            Assert.Equal(1, metricsFactory.GetCounter("jaeger:started_spans", "sampled=y"));
            Assert.Equal(0, metricsFactory.GetCounter("jaeger:started_spans", "sampled=n"));
            Assert.Equal(1, metricsFactory.GetCounter("jaeger:traces", "sampled=y,state=started"));
            Assert.Equal(0, metricsFactory.GetCounter("jaeger:traces", "sampled=n,state=started"));
        }
    }
}
