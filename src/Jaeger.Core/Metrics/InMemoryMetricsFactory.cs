using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Jaeger.Core.Metrics
{
    /// <summary>
    /// An ephemeral metrics factory, storing data in memory. This metrics factory is not meant to be used for
    /// production purposes.
    /// </summary>
    public class InMemoryMetricsFactory : IMetricsFactory
    {
        private readonly ConcurrentDictionary<string, AtomicLong> _counters = new ConcurrentDictionary<string, AtomicLong>();
        private readonly ConcurrentDictionary<string, AtomicLong> _timers = new ConcurrentDictionary<string, AtomicLong>();
        private readonly ConcurrentDictionary<string, AtomicLong> _gauges = new ConcurrentDictionary<string, AtomicLong>();

        public ICounter CreateCounter(string name, Dictionary<string, string> tags)
        {
            return new InMemoryMetric(_counters.GetOrAdd(MetricsImpl.AddTagsToMetricName(name, tags), _ => new AtomicLong()));
        }

        public ITimer CreateTimer(string name, Dictionary<string, string> tags)
        {
            return new InMemoryMetric(_timers.GetOrAdd(MetricsImpl.AddTagsToMetricName(name, tags), _ => new AtomicLong()));
        }

        public IGauge CreateGauge(string name, Dictionary<string, string> tags)
        {
            return new InMemoryMetric(_gauges.GetOrAdd(MetricsImpl.AddTagsToMetricName(name, tags), _ => new AtomicLong()));
        }

        /// <summary>
        /// Returns the counter value information for the counter with the given metric name.
        /// Note that the metric name is not the counter name, as a metric name usually includes the tags.
        /// </summary>
        /// <param name="name">The metric name, which includes the tags.</param>
        /// <param name="tags">The metric tags as comma separated list of entries, like "foo=bar,baz=qux".</param>
        /// <returns>The counter value or -1, if no counter exists for the given metric name.</returns>
        public long GetCounter(string name, string tags)
        {
            return GetValue(_counters, name, tags);
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
            return GetValue(_counters, name, tags);
        }

        /// <summary>
        /// Returns the current value for the gauge with the given metric name. Note that the metric name is not the gauge
        /// name, as a metric name usually includes the tags.
        /// </summary>
        /// <param name="name">The metric name, which includes the tags.</param>
        /// <param name="tags">The metric tags as comma separated list of entries, like "foo=bar,baz=qux".</param>
        /// <returns>The gauge value or -1, if no gauge exists for the given metric name.</returns>
        public long GetGauge(string name, string tags)
        {
            return GetValue(_gauges, name, tags);
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
            return GetValue(_gauges, name, tags);
        }

        /// <summary>
        /// Returns the current accumulated timing information for the timer with the given metric name.
        /// Note that the metric name is not the timer name, as a metric name usually includes the tags.
        /// </summary>
        /// <param name="name">The metric name, which includes the tags.</param>
        /// <param name="tags">The metric tags as comma separated list of entries, like "foo=bar,baz=qux".</param>
        /// <returns>The timer value or -1, if no timer exists for the given metric name.</returns>
        public long GetTimer(string name, string tags)
        {
            return GetValue(_timers, name, tags);
        }

        /// <summary>
        /// Returns the current accumulated timing information for the timer with the given metric name.
        /// Note that the metric name is not the timer name, as a metric name usually includes the tags.
        /// </summary>
        /// <param name="name">The metric name, which includes the tags.</param>
        /// <param name="tags">The metric tags.</param>
        /// <returns>The timer value or -1, if no timer exists for the given metric name.</returns>
        public long GetTimer(string name, Dictionary<string, string> tags)
        {
            return GetValue(_timers, name, tags);
        }

        private long GetValue(IDictionary<string, AtomicLong> collection, string name, string tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return GetValue(collection, name);
            }

            string[] entries = tags.Split(',');
            Dictionary<string, string> tagsAsMap = new Dictionary<string, string>(entries.Length);
            foreach (string entry in entries)
            {
                string[] keyValue = entry.Split('=');
                if (keyValue.Length == 2)
                {
                    tagsAsMap[keyValue[0]] = keyValue[1];
                }
                else
                {
                    tagsAsMap[keyValue[0]] = "";
                }
            }

            return GetValue(collection, MetricsImpl.AddTagsToMetricName(name, tagsAsMap));
        }

        private long GetValue(IDictionary<string, AtomicLong> collection, string name, Dictionary<string, string> tags)
        {
            return GetValue(collection, MetricsImpl.AddTagsToMetricName(name, tags));
        }

        private long GetValue(IDictionary<string, AtomicLong> collection, string name)
        {
            if (collection.TryGetValue(name, out AtomicLong value))
            {
                return value.Value;
            }
            else
            {
                return -1;
            }
        }

        private class AtomicLong
        {
            private long _value = 0;

            public long Value => _value;

            public long Add(long value)
            {
                return Interlocked.Add(ref _value, value);
            }
        }

        private class InMemoryMetric : ICounter, ITimer, IGauge
        {
            private readonly AtomicLong _atomicLong;

            public InMemoryMetric(AtomicLong atomicLong)
            {
                _atomicLong = atomicLong;
            }

            public void DurationTicks(long ticks)
            {
                _atomicLong.Add(ticks);
            }

            public void Inc(long delta)
            {
                _atomicLong.Add(delta);
            }

            public void Update(long amount)
            {
                _atomicLong.Add(amount);
            }
        }
    }
}
