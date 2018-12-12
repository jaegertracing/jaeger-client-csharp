using Jaeger.Metrics;
using System.Collections.Generic;
using Prometheus.Advanced;
using Xunit;
using ICounter = Jaeger.Metrics.ICounter;
using IGauge = Jaeger.Metrics.IGauge;

namespace Jaeger.Prometheus.Tests
{
    public class PrometheusMetricsFactoryTest
    {
        public PrometheusMetricsFactoryTest()
        {
            DefaultCollectorRegistry.Instance.Clear();
        }

        [Fact]
        public void MetricNameIsUsedForCounter()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            PrometheusMetricsFactory prometheusMetricsFactory = new PrometheusMetricsFactory();
            prometheusMetricsFactory.CreateCounter("thecounter", tags);

            Assert.Equal(-1, prometheusMetricsFactory.GetCounter("thecounter", new Dictionary<string, string>()));
            Assert.Equal(0, prometheusMetricsFactory.GetCounter("thecounter", tags));
        }

        [Fact]
        public void CounterValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            PrometheusMetricsFactory prometheusMetricsFactory = new PrometheusMetricsFactory();
            ICounter counter = prometheusMetricsFactory.CreateCounter("thecounter", tags);
            Assert.Equal(0, prometheusMetricsFactory.GetCounter("thecounter", tags));

            counter.Inc(1);

            Assert.Equal(1, prometheusMetricsFactory.GetCounter("thecounter", tags));
        }

        //[Fact]
        //public void MetricNameIsUsedForTimer()
        //{
        //    var tags = new Dictionary<string, string> { { "foo", "bar" } };

        //    PrometheusMetricsFactory prometheusMetricsFactory = new PrometheusMetricsFactory();
        //    prometheusMetricsFactory.CreateTimer("thetimer", tags);

        //    Assert.Equal(-1, prometheusMetricsFactory.GetTimer("thetimer", new Dictionary<string, string>()));
        //    Assert.Equal(0, prometheusMetricsFactory.GetTimer("thetimer", tags));
        //}

        //[Fact]
        //public void TimerValueIsIncreased()
        //{
        //    var tags = new Dictionary<string, string> { { "foo", "bar" } };

        //    PrometheusMetricsFactory prometheusMetricsFactory = new PrometheusMetricsFactory();
        //    ITimer timer = prometheusMetricsFactory.CreateTimer("thetimer", tags);
        //    Assert.Equal(0, prometheusMetricsFactory.GetTimer("thetimer", tags));

        //    timer.DurationTicks(1);

        //    Assert.Equal(1, prometheusMetricsFactory.GetTimer("thetimer", tags));
        //}

        [Fact]
        public void MetricNameIsUsedForGauge()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            PrometheusMetricsFactory prometheusMetricsFactory = new PrometheusMetricsFactory();
            prometheusMetricsFactory.CreateGauge("thegauge", tags);

            Assert.Equal(-1, prometheusMetricsFactory.GetGauge("thegauge", new Dictionary<string, string>()));
            Assert.Equal(0, prometheusMetricsFactory.GetGauge("thegauge", tags));
        }

        [Fact]
        public void GaugeValueIsIncreased()
        {
            var tags = new Dictionary<string, string> { { "foo", "bar" } };

            PrometheusMetricsFactory prometheusMetricsFactory = new PrometheusMetricsFactory();
            IGauge gauge = prometheusMetricsFactory.CreateGauge("thegauge", tags);
            Assert.Equal(0, prometheusMetricsFactory.GetGauge("thegauge", tags));

            gauge.Update(1);

            Assert.Equal(1, prometheusMetricsFactory.GetGauge("thegauge", tags));
        }

        //[Fact]
        //public void EmptyValueForTag()
        //{
        //    PrometheusMetricsFactory metricsFactory = new PrometheusMetricsFactory();
        //    Tracer tracer = new Tracer.Builder("metricsFactoryTest")
        //        .WithReporter(new NoopReporter())
        //        .WithSampler(new ConstSampler(true))
        //        .WithMetrics(new MetricsImpl(metricsFactory))
        //        .Build();

        //    tracer.BuildSpan("theoperation").Start();
        //    Assert.Equal(-1, metricsFactory.GetCounter("jaeger:started_spans", "sampled"));
        //    Assert.Equal(-1, metricsFactory.GetCounter("jaeger:started_spans", ""));
        //}

        //[Fact]
        //public void CanBeUsedWithMetrics()
        //{
        //    PrometheusMetricsFactory metricsFactory = new PrometheusMetricsFactory();
        //    Tracer tracer = new Tracer.Builder("metricsFactoryTest")
        //        .WithReporter(new NoopReporter())
        //        .WithSampler(new ConstSampler(true))
        //        .WithMetrics(new MetricsImpl(metricsFactory))
        //        .Build();

        //    tracer.BuildSpan("theoperation").Start();
        //    Assert.Equal(1, metricsFactory.GetCounter("jaeger:started_spans", "sampled=y"));
        //    Assert.Equal(0, metricsFactory.GetCounter("jaeger:started_spans", "sampled=n"));
        //    Assert.Equal(1, metricsFactory.GetCounter("jaeger:traces", "sampled=y,state=started"));
        //    Assert.Equal(0, metricsFactory.GetCounter("jaeger:traces", "sampled=n,state=started"));
        //}
    }
}
