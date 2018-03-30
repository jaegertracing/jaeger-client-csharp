using OpenTracing;

namespace Jaeger.Core.Propagation
{
    public interface IInjector
    {
        void Inject<TCarrier>(ISpanContext spanContext, TCarrier carrier);
    }
}