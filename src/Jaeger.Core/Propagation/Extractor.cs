namespace Jaeger.Core.Propagation
{
    public abstract class Extractor<TCarrier> : IExtractor
    {
        public SpanContext Extract(object carrier)
        {
            return Extract((TCarrier)carrier);
        }

        protected abstract SpanContext Extract(TCarrier carrier);
    }
}