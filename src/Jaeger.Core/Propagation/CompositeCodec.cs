using System.Collections.Generic;
using System.Text;

namespace Jaeger.Core.Propagation
{
    public class CompositeCodec<TCarrier> : Codec<TCarrier>
    {
        private readonly List<Codec<TCarrier>> _codecs;

        public CompositeCodec(List<Codec<TCarrier>> codecs)
        {
            _codecs = new List<Codec<TCarrier>>(codecs);
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            foreach (Codec<TCarrier> codec in _codecs)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append(" : ");
                }
                buffer.Append(codec.ToString());
            }
            return buffer.ToString();
        }

        protected override void Inject(SpanContext spanContext, TCarrier carrier)
        {
            foreach (Codec<TCarrier> codec in _codecs)
            {
                codec.Inject(spanContext, carrier);
            }
        }

        protected override SpanContext Extract(TCarrier carrier)
        {
            foreach (Codec<TCarrier> codec in _codecs)
            {
                SpanContext context = codec.Extract(carrier);
                if (context != null)
                {
                    return context;
                }
            }
            return null;
        }
    }
}