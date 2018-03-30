using OpenTracing;
using OpenTracing.Propagation;

namespace Jaeger.Core.Propagation
{
    public interface IPropagationRegistry
    {
        void AddCodec<TCarrier>(IFormat<TCarrier> format, IInjector injector, IExtractor extractor);
        void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier);
        ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier);
    }
}