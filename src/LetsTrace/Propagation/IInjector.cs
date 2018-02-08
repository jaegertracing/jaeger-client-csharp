using OpenTracing;

namespace LetsTrace.Propagation
{
    public interface IInjector
    {
        void Inject<TCarrier>(ISpanContext spanContext, TCarrier carrier);
    }
}