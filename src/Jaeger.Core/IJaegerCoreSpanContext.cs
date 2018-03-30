using OpenTracing;

namespace Jaeger.Core
{
    public interface IJaegerCoreSpanContext : ISpanContext
    {
        TraceId TraceId { get; }
        SpanId SpanId { get; }
        SpanId ParentId { get; }
        ContextFlags Flags { get; }
        bool IsSampled { get; }
    }
}