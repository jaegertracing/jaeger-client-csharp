using System.Threading;

namespace LetsTrace.Metrics
{
    public class InMemoryMetricsFactory : BaseMetricsFactory
    {
        public static readonly InMemoryMetricsFactory Instance = new InMemoryMetricsFactory();

        private InMemoryMetricsFactory()
        {
        }

        private class InMemoryElement : ICounter, ITimer, IGauge
        {
            private long value;

            public long Value => value;
            public long Count => value;
            public long MillisecondsTotal => value;

            public string Name { get; }
            public MetricAttribute Attribute { get; }

            public InMemoryElement(string name, MetricAttribute attribute)
            {
                Name = name;
                Attribute = attribute;
            }

            public void Inc(long delta)
            {
                Interlocked.Add(ref value, delta);
            }

            public void DurationMicros(long time)
            {
                Interlocked.Add(ref value, time);
            }

            public void Update(long amount)
            {
                Interlocked.Add(ref value, amount);
            }

            public override string ToString()
            {
                return $"{Name}: {Value}";
            }
        }

        protected override ICounter CreateCounter(string name, MetricAttribute attribute)
        {
            return new InMemoryElement(name, attribute);
        }

        protected override ITimer CreateTimer(string name, MetricAttribute attribute)
        {
            return new InMemoryElement(name, attribute);
        }

        protected override IGauge CreateGauge(string name, MetricAttribute attribute)
        {
            return new InMemoryElement(name, attribute);
        }
    }
}
