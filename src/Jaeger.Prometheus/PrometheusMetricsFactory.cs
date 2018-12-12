using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Prometheus;

namespace Jaeger.Metrics
{
    /// <summary>
    /// An ephemeral metrics factory, storing data in Prometheus.
    /// </summary>
    public class PrometheusMetricsFactory : IMetricsFactory
    {
        private readonly ConcurrentDictionary<string, Counter> _counters = new ConcurrentDictionary<string, Counter>();
        private readonly ConcurrentDictionary<string, Histogram> _timers = new ConcurrentDictionary<string, Histogram>();
        private readonly ConcurrentDictionary<string, Gauge> _gauges = new ConcurrentDictionary<string, Gauge>();
        
        public ICounter CreateCounter(string name, Dictionary<string, string> tags)
        {
            return new PrometheusCounter(_counters.GetOrAdd(name, _ => PrometheusCounter.Create(name, tags)), tags);
        }

        public ITimer CreateTimer(string name, Dictionary<string, string> tags)
        {
            return new PrometheusTimer(_timers.GetOrAdd(name, _ => PrometheusTimer.Create(name, tags)), tags);
        }

        public IGauge CreateGauge(string name, Dictionary<string, string> tags)
        {
            return new PrometheusGauge(_gauges.GetOrAdd(name, _ => PrometheusGauge.Create(name, tags)), tags);
        }

        /// <summary>
        /// Returns the counter value information for the counter with the given metric name.
        /// Note that the metric name is not the counter name, as a metric name usually includes the tags.
        /// </summary>
        /// <param name="name">The metric name, which includes the tags.</param>
        /// <param name="tags">The metric tags.</param>
        /// <returns>The counter value or -1, if no counter exists for the given metric name.</returns>
        public long GetCounter(string name, Dictionary<string, string> tags)
        {
            if (_counters.TryGetValue(name, out var value) && value.LabelNames.All(tags.ContainsKey))
            {
                return Convert.ToInt64(value.WithLabels(value.LabelNames.Select(k => tags[k]).ToArray()).Value);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns the current value for the gauge with the given metric name. Note that the metric name is not the gauge
        /// name, as a metric name usually includes the tags.
        /// </summary>
        /// <param name="name">The metric name, which includes the tags.</param>
        /// <param name="tags">The metric tags.</param>
        /// <returns>The gauge value or -1, if no gauge exists for the given metric name.</returns>
        public long GetGauge(string name, Dictionary<string, string> tags)
        {
            if (_gauges.TryGetValue(name, out var value) && value.LabelNames.All(tags.ContainsKey))
            {
                return Convert.ToInt64(value.WithLabels(value.LabelNames.Select(k => tags[k]).ToArray()).Value);
            }
            else
            {
                return -1;
            }
        }

        private class PrometheusCounter : ICounter
        {
            private readonly Prometheus.ICounter _counter;

            public PrometheusCounter(Counter counter, Dictionary<string, string> tags)
            {
                _counter = counter.WithLabels(tags.Values.ToArray());
            }

            public static Counter Create(string name, Dictionary<string, string> tags)
            {
                return Prometheus.Metrics.CreateCounter(name, null, tags.Keys.ToArray());
            }

            public void Inc(long delta)
            {
                _counter.Inc(delta);
            }
        }

        private class PrometheusTimer : ITimer
        {
            private readonly IHistogram _histogram;

            public PrometheusTimer(Histogram histogram, Dictionary<string, string> tags)
            {
                _histogram = histogram.WithLabels(tags.Values.ToArray());
            }

            public static Histogram Create(string name, Dictionary<string, string> tags)
            {
                return Prometheus.Metrics.CreateHistogram(name, null, new HistogramConfiguration {LabelNames = tags.Keys.ToArray()});
            }

            public void DurationTicks(long ticks)
            {
                _histogram.Observe(new TimeSpan(ticks).TotalMilliseconds);
            }
        }

        private class PrometheusGauge : IGauge
        {
            private readonly Prometheus.IGauge _gauge;

            public PrometheusGauge(Gauge gauge, Dictionary<string, string> tags)
            {
                _gauge = gauge.WithLabels(tags.Values.ToArray());
            }

            public static Gauge Create(string name, Dictionary<string, string> tags)
            {
                return Prometheus.Metrics.CreateGauge(name, null, tags.Keys.ToArray());
            }

            public void Update(long amount)
            {
                _gauge.Inc(amount);
            }
        }
    }
}
