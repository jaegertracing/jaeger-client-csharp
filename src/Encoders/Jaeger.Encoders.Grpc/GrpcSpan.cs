namespace Jaeger.Encoders.Grpc
{
    public class GrpcSpan : EncodedData, IEncodedSpan
    {
        public ApiV2.Span Span { get; }
        public override object Data => Span;

        public GrpcSpan(ApiV2.Span span)
        {
            Span = span;
        }
    }
}
