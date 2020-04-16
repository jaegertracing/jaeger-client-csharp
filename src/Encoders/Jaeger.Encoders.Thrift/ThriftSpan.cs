namespace Jaeger.Encoders.Thrift
{
    public class ThriftSpan : EncodedData, IEncodedSpan
    {
        public Jaeger.Thrift.Span Span { get; }
        public override object Data => Span;

        public ThriftSpan(Jaeger.Thrift.Span span)
        {
            Span = span;
        }
    }
}
