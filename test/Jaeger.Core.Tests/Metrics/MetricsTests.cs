using Jaeger.Core.Metrics;
using System.Collections.Generic;
using Xunit;

namespace Jaeger.Core.Tests.Metrics
{
    public class MetricsTests
    {
        private readonly InMemoryMetricsFactory _metricsFactory;
        private readonly IMetrics _metrics;

        public MetricsTests()
        {
            _metricsFactory = new InMemoryMetricsFactory();
            _metrics = new MetricsImpl(_metricsFactory);
        }

        [Fact]
        public void TestCounterWithoutExplicitTags()
        {
            _metrics.TracesJoinedSampled.Inc(1);
            Assert.Equal(1, _metricsFactory.GetCounter("jaeger:traces", "sampled=y,state=joined"));
        }

        [Fact]
        public void TestGaugeWithoutExplicitTags()
        {
            _metrics.ReporterQueueLength.Update(1);
            Assert.Equal(1, _metricsFactory.GetGauge("jaeger:reporter_queue_length", ""));
        }

        [Fact]
        public void TestAddTagsToMetricName()
        {
            var tags = new Dictionary<string, string>();
            tags["foo"] = "bar";
            Assert.Equal("thecounter.foo=bar", MetricsImpl.AddTagsToMetricName("thecounter", tags));
            Assert.Equal("jaeger:thecounter.foo=bar", MetricsImpl.AddTagsToMetricName("jaeger:thecounter", tags));
        }
    }
}
