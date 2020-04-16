using Jaeger.Encoders.SizedBatch;
using Jaeger.Thrift;

namespace Jaeger.Encoders.Thrift
{
    public class ThriftProcess : EncodedData, IEncodedProcess
    {
        public Process Process { get; }
        public override object Data => Process;

        public ThriftProcess(Process process)
        {
            Process = process;
        }
    }
}
