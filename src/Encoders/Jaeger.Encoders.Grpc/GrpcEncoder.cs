using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Encoders.Grpc.Internal;
using Jaeger.Encoders.SizedBatch;
using Jaeger.Transports;
using Jaeger.Transports.Grpc;

namespace Jaeger.Encoders.Grpc
{
    public class GrpcEncoder : IExtendedEncoder
    {
        public GrpcTransport Transport { get; }
        ITransport IEncoder.Transport => Transport;

        public GrpcEncoder(GrpcTransport transport)
        {
            Transport = transport;
        }

        public IEncodedSpan GetSpan(Span span)
        {
            return new GrpcSpan(JaegerGrpcSpanConverter.ConvertSpan(span));
        }

        public IEncodedProcess GetProcess(Span span)
        {
            return new GrpcProcess(JaegerGrpcSpanConverter.ConvertProcess(span));
        }

        public IEncodedBatch GetBatch(IEncodedProcess process, IEnumerable<IEncodedSpan> spans)
        {
            return new GrpcBatch(JaegerGrpcSpanConverter.ConvertBatch(process, spans));
        }

        public Task WriteBatchAsync(IEncodedBatch batch, CancellationToken cancellationToken)
        {
            var encBatch = ((GrpcBatch) batch).Batch;
            return Transport.WriteBatchAsync(encBatch, cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(GrpcEncoder)}(Transport={Transport})";
        }
    }
}
