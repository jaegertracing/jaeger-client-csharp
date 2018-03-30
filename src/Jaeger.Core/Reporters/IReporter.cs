using System;

namespace Jaeger.Core.Reporters
{
    // Used by the Tracer to submit spans when they are finished
    public interface IReporter : IDisposable
    {
        // Report submits a finished span to be recorded - possibly asynchronously or buffered
        void Report(IJaegerCoreSpan span);
    }
}
