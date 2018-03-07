using OpenTracing;
using OpenTracing.Propagation;

namespace LetsTrace.Propagation
{
    public interface IPropagator
    {
        void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier);
        ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier);
    }
}