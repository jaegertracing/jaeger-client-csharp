using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Core.Senders
{
    public interface ISender
    {
        Task<int> AppendAsync(Span span, CancellationToken cancellationToken);

        Task<int> FlushAsync(CancellationToken cancellationToken);

        Task<int> CloseAsync(CancellationToken cancellationToken);
    }
}