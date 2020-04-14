namespace Jaeger.Metrics
{
    public interface IGauge
    {
        void Update(long amount);
    }
}
