namespace LetsTrace.Reporters
{
    public class NullReporter : IReporter
    {
        public void Dispose() {}

        public void Report(ILetsTraceSpan span) {}

    }
}
