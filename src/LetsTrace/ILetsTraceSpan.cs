using System;
using System.Collections.Generic;
using OpenTracing;

namespace LetsTrace
{
    public interface ILetsTraceSpan : ISpan
    {
        DateTimeOffset? FinishTimestamp { get; }
        List<LogRecord> Logs { get; }
        string OperationName { get; }
        List<Reference> References { get; }
        DateTimeOffset StartTimestamp { get; }
        Dictionary<string, string> Tags { get; }
        ILetsTraceTracer Tracer { get; }
    }
}
