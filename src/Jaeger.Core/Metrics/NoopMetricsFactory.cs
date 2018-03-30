using System.Diagnostics.CodeAnalysis;

namespace Jaeger.Core.Metrics
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
            // TODO: Not used now, but ported from java implementation
            [ExcludeFromCodeCoverage]
            public long MillisecondsTotal { get; }
            public long Value { get; }

            public void Inc(long delta)
            {
            }

            // TODO: Not used now, but ported from java implementation
            [ExcludeFromCodeCoverage]
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

        // TODO: Not used now, but ported from java implementation
        [ExcludeFromCodeCoverage]
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
