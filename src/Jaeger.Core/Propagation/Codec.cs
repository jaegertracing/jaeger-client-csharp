namespace Jaeger.Core.Propagation
{
    public abstract class Codec<TCarrier> : ICodec
    {
        public void Inject(SpanContext spanContext, object carrier)
        {
            Inject(spanContext, (TCarrier)carrier);
        }

        public SpanContext Extract(object carrier)
        {
            return Extract((TCarrier)carrier);
        }

        protected abstract void Inject(SpanContext spanContext, TCarrier carrier);

        protected abstract SpanContext Extract(TCarrier carrier);
    }
}