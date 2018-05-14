namespace Jaeger.Metrics
{
    public interface ICounter
    {
        void Inc(long delta);
    }
}
