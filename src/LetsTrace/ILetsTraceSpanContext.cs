using OpenTracing;

namespace LetsTrace
{
    public interface ILetsTraceSpanContext : ISpanContext
    {
        TraceId TraceId { get; }
        SpanId SpanId { get; }
        SpanId ParentId { get; }
        ContextFlags Flags { get; }
        bool IsSampled { get; }
    }
}