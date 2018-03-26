using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenTracing;

namespace LetsTrace
{
    public interface ILetsTraceSpan : ISpan, IDisposable
    {
        new ILetsTraceSpanContext Context { get; }
        DateTime? FinishTimestampUtc { get; }
        List<LogRecord> Logs { get; }
        string OperationName { get; }
        IEnumerable<Reference> References { get; }
        DateTime StartTimestampUtc { get; }
        Dictionary<string, Field> Tags { get; }
        [JsonIgnore] ILetsTraceTracer Tracer { get; }
    }
}
