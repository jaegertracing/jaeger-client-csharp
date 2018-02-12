using LetsTrace.Util;
using OpenTracing;

namespace LetsTrace
{
    public interface ILetsTraceTracer : ITracer
    {
        IClock Clock { get; }
        string HostIPv4 { get; }
        void ReportSpan(ILetsTraceSpan span);
        string ServiceName { get; }
        ILetsTraceSpan SetBaggageItem(ILetsTraceSpan span, string key, string value);
    }
}
