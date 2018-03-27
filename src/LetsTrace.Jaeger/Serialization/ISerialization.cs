using JaegerSpan = Jaeger.Thrift.Span;
using JaegerProcess = Jaeger.Thrift.Process;

namespace LetsTrace.Jaeger.Serialization
{
    public interface ISerialization
    {
        JaegerSpan BuildJaegerThriftSpan(ILetsTraceSpan span);
        JaegerProcess BuildJaegerProcessThrift(ILetsTraceTracer tracer);
    }
}
