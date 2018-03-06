using System;
using System.Collections.Generic;
using OpenTracing;

namespace LetsTrace
{
    public interface ILetsTraceSpan : ISpan, IDisposable
    {
        DateTimeOffset? FinishTimestamp { get; }
        List<LogRecord> Logs { get; }
        string OperationName { get; }
        List<Reference> References { get; }
        DateTimeOffset StartTimestamp { get; }
        Dictionary<string, Field> Tags { get; }
        ILetsTraceTracer Tracer { get; }
    }
}
