using OpenTracing;

namespace Jaeger.Core.Propagation
{
    public interface IExtractor
    {
        ISpanContext Extract<TCarrier>(TCarrier carrier);
    }
}