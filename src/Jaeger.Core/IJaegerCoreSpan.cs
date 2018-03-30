using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenTracing;

namespace Jaeger.Core
{
    public interface IJaegerCoreSpan : ISpan, IDisposable
    {
        new IJaegerCoreSpanContext Context { get; }
        DateTime? FinishTimestampUtc { get; }
        List<LogRecord> Logs { get; }
        string OperationName { get; }
        IEnumerable<Reference> References { get; }
        DateTime StartTimestampUtc { get; }
        Dictionary<string, object> Tags { get; }
        [JsonIgnore] IJaegerCoreTracer Tracer { get; }
    }
}
