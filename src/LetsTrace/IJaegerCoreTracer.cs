using System;
using System.Collections.Generic;
using Jaeger.Core.Util;
using OpenTracing;

namespace Jaeger.Core
{
    public interface IJaegerCoreTracer : ITracer, IDisposable
    {
        Dictionary<string, object> Tags { get; }
        IClock Clock { get; }
        string HostIPv4 { get; }
        string ServiceName { get; }
        void ReportSpan(IJaegerCoreSpan span);
        IJaegerCoreSpan SetBaggageItem(IJaegerCoreSpan span, string key, string value);
    }
}
