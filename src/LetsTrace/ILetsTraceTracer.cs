using LetsTrace.Util;
using OpenTracing;

namespace LetsTrace
{
    public interface ILetsTraceTracer : ITracer
    {
        IClock Clock { get; }
        void ReportSpan(ILetsTraceSpan span);
        ILetsTraceSpan SetBaggageItem(ILetsTraceSpan span, string key, string value);
    }
}
