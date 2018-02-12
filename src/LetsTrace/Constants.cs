using LetsTrace.Propagation;

namespace LetsTrace
{
    public class Constants
    {
        public const string TraceContextHeaderName = "X-LetsTrace-Trace-Context";
        public const string TraceBaggageHeaderPrefix = "X-LetsTrace-Baggage";
    }
}