using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Encoders;
using Jaeger.Encoders.SizedBatch;
using Jaeger.Exceptions;

namespace Jaeger.Senders.SizedBatch
{
    public abstract class SizedBatchSender : ISender
    {
        protected readonly IExtendedEncoder _encoder;
        protected readonly int _maxPacketSize;

        private readonly List<IEncodedSpan> _spanBuffer = new List<IEncodedSpan>();
        private IEncodedProcess _process;
        private int _processBytesSize;
        private int _maxSpanSize;
        private int _byteBufferSize;

        public SizedBatchSender(IExtendedEncoder encoder, int maxPacketSize)
        {
            _encoder = encoder;
            _maxPacketSize = maxPacketSize;
        }

        public async Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            if (_process == null)
            {
                _process = _encoder.GetProcess(span);
                _processBytesSize = _process.GetSize(_encoder);
                _byteBufferSize += _processBytesSize;
                _maxSpanSize = _maxPacketSize - _processBytesSize;

                if (_maxSpanSize < 0)
                {
                    throw new SenderException($"{nameof(SizedBatchSender)} was misconfigured, packet size too small, size = {_maxPacketSize}, max = {_processBytesSize}", null, 1);
                }
            }

            var encSpan = _encoder.GetSpan(span);
            int spanSize = encSpan.GetSize(_encoder);
            if (spanSize > _maxSpanSize)
            {
                throw new SenderException($"{nameof(SizedBatchSender)} received a span that was too large, size = {spanSize}, max = {_maxSpanSize}", null, 1);
            }

            _byteBufferSize += spanSize;
            if (_byteBufferSize <= _maxPacketSize)
            {
                _spanBuffer.Add(encSpan);
                if (_byteBufferSize < _maxPacketSize)
                {
                    return 0;
                }
                return await FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            int n;
            try
            {
                n = await FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (SenderException ex)
            {
                // +1 for the span not submitted in the buffer above
                throw new SenderException(ex.Message, ex, ex.DroppedSpanCount + 1);
            }

            _spanBuffer.Add(encSpan);
            _byteBufferSize = _processBytesSize + spanSize;
            return n;
        }

        public async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            int n = _spanBuffer.Count;
            if (n == 0)
            {
                return 0;
            }

            try
            {
                var batch = _encoder.GetBatch(_process, _spanBuffer);
                await _encoder.WriteBatchAsync(batch, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new SenderException("Failed to flush spans.", ex, n);
            }
            finally
            {
                _spanBuffer.Clear();
                _byteBufferSize = _processBytesSize;
            }
            return n;
        }

        public virtual Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return FlushAsync(cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(SizedBatchSender)}(Encoder={_encoder}, ProcessBytesSize={_processBytesSize}, ByteBufferSize={_byteBufferSize})";
        }
    }
}