namespace Jaeger.Core.Metrics
{
    public interface ICounter
    {
        void Inc(long delta);
    }
}
