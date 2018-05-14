namespace Jaeger.Core.Propagation
{
    public abstract class Injector<TCarrier> : IInjector
    {
        public void Inject(SpanContext spanContext, object carrier)
        {
            Inject(spanContext, (TCarrier)carrier);
        }

        protected abstract void Inject(SpanContext spanContext, TCarrier carrier);
    }
}