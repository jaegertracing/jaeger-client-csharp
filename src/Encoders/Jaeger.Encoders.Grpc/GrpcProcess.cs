using Jaeger.ApiV2;
using Jaeger.Encoders.SizedBatch;

namespace Jaeger.Encoders.Grpc
{
    public class GrpcProcess : EncodedData, IEncodedProcess
    {
        public Process Process { get; }
        public override object Data => Process;

        public GrpcProcess(Process process)
        {
            Process = process;
        }
    }
}
