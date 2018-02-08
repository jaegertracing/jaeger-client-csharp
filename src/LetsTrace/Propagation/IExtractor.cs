using OpenTracing;

namespace LetsTrace.Propagation
{
    public interface IExtractor
    {
        ISpanContext Extract<TCarrier>(TCarrier carrier);
    }
}