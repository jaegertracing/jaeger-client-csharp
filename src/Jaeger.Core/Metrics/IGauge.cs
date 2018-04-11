namespace Jaeger.Core.Metrics
{
    public interface IGauge
    {
        void Update(long amount);
    }
}
