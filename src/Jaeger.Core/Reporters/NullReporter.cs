using System.Diagnostics.CodeAnalysis;

namespace Jaeger.Core.Reporters
{
    [ExcludeFromCodeCoverage]
    public class NullReporter : IReporter
    {
        public void Dispose() {}

        public void Report(IJaegerCoreSpan span) {}

    }
}
