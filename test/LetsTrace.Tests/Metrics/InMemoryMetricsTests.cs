using System;
using System.Collections.Generic;
using System.Globalization;
using LetsTrace.Metrics;
using Xunit;

namespace LetsTrace.Tests.Metrics
{
    public class InMemoryMetricsTests
    {
        private readonly InMemoryMetricsFactory _factory;

        public InMemoryMetricsTests()
        {
            _factory = InMemoryMetricsFactory.Instance;
        }

        [Fact]
        public void InMemoryMetrics_ShouldBeSeparateInstances()
        {
            var metrics1 = _factory.CreateMetrics();
            var metrics2 = _factory.CreateMetrics();

            Assert.False(ReferenceEquals(metrics1, metrics2));
            Assert.Equal(metrics1.SpansStartedSampled.Count, metrics2.SpansStartedSampled.Count);
            Assert.Equal(metrics1.SpansFinished.Count, metrics2.SpansFinished.Count);

            metrics1.SpansFinished.Inc(1);
            Assert.NotEqual(metrics1.SpansFinished.Count, metrics2.SpansFinished.Count);
        }

        [Fact]
        public void InMemoryMetrics_ShouldHaveCorrectValues()
        {
            var metrics = _factory.CreateMetrics();

            var metric = metrics.TraceStartedSampled;
            Assert.Equal(nameof(metrics.TraceStartedSampled), metric.Name);
            Assert.Equal("traces", metric.Attribute.Name);
            Assert.Equal(MetricAttribute.MetricState.Started, metric.Attribute.State);
            Assert.Equal(MetricAttribute.MetricSampled.Yes, metric.Attribute.Sampled);
            Assert.Equal(MetricAttribute.MetricResult.Undefined, metric.Attribute.Result);

            metric = metrics.ReporterSuccess;
            Assert.Equal(nameof(metrics.ReporterSuccess), metric.Name);
            Assert.Equal("reporter_spans", metric.Attribute.Name);
            Assert.Equal(MetricAttribute.MetricState.Undefined, metric.Attribute.State);
            Assert.Equal(MetricAttribute.MetricSampled.Undefined, metric.Attribute.Sampled);
            Assert.Equal(MetricAttribute.MetricResult.Ok, metric.Attribute.Result);
        }

        [Fact]
        public void InMemoryMetrics_ShouldReturnReadableValue()
        {
            var metrics = _factory.CreateMetrics();
            var metric = metrics.TraceStartedSampled;

            metric.Inc(10);
            Assert.Equal("TraceStartedSampled: 10", metric.ToString());
        }

        [Fact]
        public void InMemoryMetrics_ShouldUseCounter()
        {
            var metrics = _factory.CreateMetrics();
            var metric = metrics.TraceStartedSampled;

            Assert.Equal(0, metric.Count);
            metric.Inc(10);
            Assert.Equal(10, metric.Count);

            Assert.Equal(0, metrics.TraceStartedNotSampled.Count);
        }

        [Fact]
        public void InMemoryMetrics_ShouldUseGauge()
        {
            var metrics = _factory.CreateMetrics();
            var metric = metrics.ReporterQueueLength;

            Assert.Equal(0, metric.Value);
            metric.Update(10);
            Assert.Equal(10, metric.Value);
        }

        /* TODO: Testfor ShouldUseTimer missing since no property exists */
    }
}
