namespace Jaeger.Core.Metrics
{
    public interface IMetricsFactory
    {
        IMetrics CreateMetrics();
    }
}
