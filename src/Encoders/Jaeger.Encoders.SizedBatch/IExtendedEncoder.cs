using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Encoders.SizedBatch
{
    public interface IExtendedEncoder : IEncoder
    {
        IEncodedProcess GetProcess(Span span);
        IEncodedBatch GetBatch(IEncodedProcess process, IEnumerable<IEncodedSpan> spans);

        Task WriteBatchAsync(IEncodedBatch batch, CancellationToken cancellationToken);
    }
}
