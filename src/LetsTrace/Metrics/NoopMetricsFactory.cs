using System.Diagnostics.CodeAnalysis;

namespace LetsTrace.Metrics
{
    [ExcludeFromCodeCoverage]
    public class NoopMetricsFactory : BaseMetricsFactory
    {
        public static readonly NoopMetricsFactory Instance = new NoopMetricsFactory();
        private static readonly NoopElement Dummy = new NoopElement();

        private NoopMetricsFactory()
        {
        }

        private class NoopElement : ICounter, ITimer, IGauge
        {
            public string Name { get; }
            public MetricAttribute Attribute { get; }
            public long Count { get; }
            public long MillisecondsTotal { get; }
            public long Value { get; }

            public void Inc(long delta)
            {
            }

            public void DurationMicros(long time)
            {
            }

            public void Update(long amount)
            {
            }
        }

        protected override ICounter CreateCounter(string name, MetricAttribute attribute)
        {
            return Dummy;
        }

        protected override ITimer CreateTimer(string name, MetricAttribute attribute)
        {
            return Dummy;
        }

        protected override IGauge CreateGauge(string name, MetricAttribute attribute)
        {
            return Dummy;
        }
    }
}
