using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Encoders.SizedBatch;
using Jaeger.Encoders.Thrift.Internal;
using Jaeger.Transports;
using Jaeger.Transports.Thrift;

namespace Jaeger.Encoders.Thrift
{
    public class ThriftEncoder : IExtendedEncoder
    {
        public ThriftTransport Transport { get; }
        ITransport IEncoder.Transport => Transport;

        public ThriftEncoder(ThriftTransport transport)
        {
            Transport = transport;
        }

        public IEncodedSpan GetSpan(Span span)
        {
            return new ThriftSpan(JaegerThriftSpanConverter.ConvertSpan(span));
        }

        public IEncodedProcess GetProcess(Span span)
        {
            return new ThriftProcess(JaegerThriftSpanConverter.ConvertProcess(span));
        }

        public IEncodedBatch GetBatch(IEncodedProcess process, IEnumerable<IEncodedSpan> spans)
        {
            return new ThriftBatch(JaegerThriftSpanConverter.ConvertBatch(process, spans));
        }

        public Task WriteBatchAsync(IEncodedBatch batch, CancellationToken cancellationToken)
        {
            var encBatch = ((ThriftBatch) batch).Batch;
            return Transport.WriteBatchAsync(encBatch, cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(ThriftEncoder)}(Transport={Transport})";
        }
    }
}
