using Jaeger.Encoders.SizedBatch;
using Jaeger.Thrift;

namespace Jaeger.Encoders.Thrift
{
    public class ThriftBatch : EncodedData, IEncodedBatch
    {
        public Batch Batch { get; }
        public override object Data => Batch;

        public ThriftBatch(Batch batch)
        {
            Batch = batch;
        }
    }
}
