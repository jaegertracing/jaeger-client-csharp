using System.Collections.Generic;

namespace Jaeger.Core.Metrics
{
    /// <summary>
    /// A metrics factory that implements NOOP counters, timers and gauges.
    /// </summary>
    public class NoopMetricsFactory : IMetricsFactory
    {
        public static readonly NoopMetricsFactory Instance = new NoopMetricsFactory();

        private readonly NoopMetric _metric = new NoopMetric();

        private NoopMetricsFactory()
        {
        }

        public ICounter CreateCounter(string name, Dictionary<string, string> tags)
        {
            return _metric;
        }

        public IGauge CreateGauge(string name, Dictionary<string, string> tags)
        {
            return _metric;
        }

        public ITimer CreateTimer(string name, Dictionary<string, string> tags)
        {
            return _metric;
        }

        private class NoopMetric : ICounter, ITimer, IGauge
        {
            public void Inc(long delta)
            {
            }

            public void DurationTicks(long ticks)
            {
            }

            public void Update(long amount)
            {
            }
        }
    }
}
