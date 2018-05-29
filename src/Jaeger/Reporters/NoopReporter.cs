using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Reporters
{
    public class NoopReporter : IReporter
    {
        public void Report(Span span)
        {
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return nameof(NoopReporter);
        }
    }
}
