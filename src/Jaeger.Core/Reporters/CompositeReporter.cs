using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Core.Reporters
{
    public class CompositeReporter : IReporter
    {
        private readonly List<IReporter> _reporters;

        public CompositeReporter(params IReporter[] reporters)
        {
            _reporters = new List<IReporter>(reporters);
        }

        public void Report(Span span)
        {
            foreach (var reporter in _reporters)
            {
                reporter.Report(span);
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            foreach (var reporter in _reporters)
            {
                await reporter.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
