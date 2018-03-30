using Jaeger.Core;
using JaegerSpan = Jaeger.Thrift.Span;
using JaegerProcess = Jaeger.Thrift.Process;

namespace Jaeger.Transport.Thrift.Serialization
{
    public interface ISerialization
    {
        JaegerSpan BuildJaegerThriftSpan(IJaegerCoreSpan span);
        JaegerProcess BuildJaegerProcessThrift(IJaegerCoreTracer tracer);
    }
}
