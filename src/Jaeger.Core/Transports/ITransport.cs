using Jaeger.Encoders;

namespace Jaeger.Transports
{
    public interface ITransport
    {
        int GetSize(IEncodedData data);
    }
}
