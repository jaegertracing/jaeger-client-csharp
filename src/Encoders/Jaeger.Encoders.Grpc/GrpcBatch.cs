using Jaeger.ApiV2;
using Jaeger.Encoders.SizedBatch;

namespace Jaeger.Encoders.Grpc
{
    public class GrpcBatch : EncodedData, IEncodedBatch
    {
        public Batch Batch { get; }
        public override object Data => Batch;

        public GrpcBatch(Batch batch)
        {
            Batch = batch;
        }
    }
}
