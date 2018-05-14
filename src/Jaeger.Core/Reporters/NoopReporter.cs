using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Core.Reporters
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
    }
}
