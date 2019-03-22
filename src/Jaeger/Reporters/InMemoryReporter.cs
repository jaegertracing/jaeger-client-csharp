using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Reporters
{
    public class InMemoryReporter : IReporter
    {
        private readonly object _lock = new object();
        private readonly List<Span> _spans = new List<Span>();

        public void Report(Span span)
        {
            lock (_lock)
            {
                _spans.Add(span);
            }
        }

        public IReadOnlyList<Span> GetSpans()
        {
            lock (_lock)
            {
                return new List<Span>(_spans);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _spans.Clear();
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            lock (_lock)
            {
                return $"{nameof(InMemoryReporter)}(Spans={string.Join(", ", _spans)})";
            }
        }
    }
}
