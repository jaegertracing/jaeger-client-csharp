using Jaeger.Transports;

namespace Jaeger.Encoders
{
    public interface IEncoder
    {
        ITransport Transport { get; }

        IEncodedSpan GetSpan(Span span);
    }
}
