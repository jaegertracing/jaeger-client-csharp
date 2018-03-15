using System.Collections.Generic;

namespace LetsTrace.Reporters
{
    public class CompositeReporter : IReporter
    {
        private readonly List<IReporter> _reporters;

        public CompositeReporter(params IReporter[] reporters)
        {
            _reporters = new List<IReporter>(reporters);
        }

        public void Dispose()
        {
            foreach(var reporter in _reporters)
            {
                reporter.Dispose();
            }
        }

        public void Report(ILetsTraceSpan span)
        {
            foreach(var reporter in _reporters)
            {
                reporter.Report(span);
            }
        }
    }
}
