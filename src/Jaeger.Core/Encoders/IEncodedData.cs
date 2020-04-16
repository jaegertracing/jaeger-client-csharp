namespace Jaeger.Encoders
{
    public interface IEncodedData
    {
        object Data { get; }

        int GetSize(IEncoder encoder);
    }
}
