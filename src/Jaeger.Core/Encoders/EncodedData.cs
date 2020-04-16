namespace Jaeger.Encoders
{
    public abstract class EncodedData : IEncodedData
    {
        public abstract object Data { get; }

        public int GetSize(IEncoder encoder)
        {
            return encoder.Transport.GetSize(this);
        }
    }
}
