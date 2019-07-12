using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Senders
{
    /// <summary>
    /// A sender that does not send anything, anywhere. Is used only as a fallback on systems where no senders can be selected.
    /// </summary>
    public class NoopSender : ISender
    {
        public static readonly NoopSender Instance = new NoopSender();

        public Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }

        public Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public override string ToString()
        {
            return $"{nameof(NoopSender)}()";
        }
    }
}
