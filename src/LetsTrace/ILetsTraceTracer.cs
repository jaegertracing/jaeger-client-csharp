using LetsTrace.Util;
using OpenTracing;
using System;
using System.Collections.Generic;

namespace LetsTrace
{
    public interface ILetsTraceTracer : ITracer, IDisposable
    {
        IDictionary<string, Field> Tags { get; }
        IClock Clock { get; }
        string HostIPv4 { get; }
        string ServiceName { get; }
        void ReportSpan(ILetsTraceSpan span);
        ILetsTraceSpan SetBaggageItem(ILetsTraceSpan span, string key, string value);
    }
}
