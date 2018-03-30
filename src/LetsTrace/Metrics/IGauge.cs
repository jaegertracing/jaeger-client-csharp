namespace Jaeger.Core.Metrics
{
    public interface IGauge : IMetricValue
    {
        long Value { get; }

        void Update(long amount);
    }
}
