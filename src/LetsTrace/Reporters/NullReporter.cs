using System.Diagnostics.CodeAnalysis;

namespace LetsTrace.Reporters
{
    [ExcludeFromCodeCoverage]
    public class NullReporter : IReporter
    {
        public void Dispose() {}

        public void Report(ILetsTraceSpan span) {}

    }
}
