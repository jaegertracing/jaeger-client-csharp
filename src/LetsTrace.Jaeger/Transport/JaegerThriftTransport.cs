using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LetsTrace.Exceptions;
using OpenTracing;

using LetsTrace.Util;
using LetsTrace.Transport;

using Thrift.Protocols;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;
using JaegerTag = Jaeger.Thrift.Tag;
using JaegerTagType = Jaeger.Thrift.TagType;
using JaegerLog = Jaeger.Thrift.Log;
using JaegerReference = Jaeger.Thrift.SpanRef;
using JaegerReferenceType = Jaeger.Thrift.SpanRefType;
using JaegerBatch = Jaeger.Thrift.Batch;

namespace LetsTrace.Jaeger.Transport
{
    public abstract class JaegerThriftTransport : ITransport
    {
        public const int JAEGER_THRIFT_TRANSPORT_DEFAULT_BUFFER_SIZE = 10; // TODO: Constants

        private readonly List<JaegerSpan> _buffer = new List<JaegerSpan>(); // TODO: look into making this thread safe
        protected readonly ITProtocolFactory _protocolFactory;
        private readonly int _bufferSize = 0;
        protected JaegerProcess _process = null;

        protected JaegerThriftTransport(ITProtocolFactory protocolFactory, int bufferSize = 0)
        {
            if (bufferSize <= 0)
            {
                bufferSize = JAEGER_THRIFT_TRANSPORT_DEFAULT_BUFFER_SIZE;
            }

            _protocolFactory = protocolFactory;
            _bufferSize = bufferSize;
        }

        public async Task<int> AppendAsync(ILetsTraceSpan span, CancellationToken canellationToken)
        {
            if (_process == null) {
                _process = BuildJaegerProcessThrift(span.Tracer);
            }
            var jaegerSpan = BuildJaegerThriftSpan(span);

            _buffer.Add(jaegerSpan);

            if (_buffer.Count > _bufferSize) {
                return await FlushAsync(canellationToken);
            }

            return 0;
        }

        internal static JaegerSpan BuildJaegerThriftSpan(ILetsTraceSpan span)
        {
            var context = span.Context;
            var startTime = span.StartTimestamp.ToUnixTimeMicroseconds();
            var duration = (span.FinishTimestamp?.ToUnixTimeMicroseconds() - startTime) ?? 0;

            var jaegerSpan = new JaegerSpan(
                (long)context.TraceId.Low,
                (long)context.TraceId.High,
                context.SpanId,
                context.ParentId,
                span.OperationName,
                0,
                startTime,
                duration
            )
            {
                Tags = BuildJaegerTags(span.Tags),
                Logs = span.Logs.Select(BuildJaegerLog).ToList(),
                References = span.References.Select(BuildJaegerReference).Where(r => r != null).ToList()
            };

            return jaegerSpan;
        }

        internal static List<JaegerTag> BuildJaegerTags(IDictionary<string, Field> inTags)
        {
            var tags = new List<JaegerTag>();
            foreach (var tag in inTags)
            {
                tag.Value.Key = tag.Key;
                tag.Value.Marshal(tags);
            }
            return tags;
        }

        // BuildJaegerReference builds a jaeger reference object from a lets
        // trace reference object. It returns null if the type of the lets
        // trace ref object does not exist in jaeger
        internal static JaegerReference BuildJaegerReference(Reference reference)
        {
            if (reference.Type != References.ChildOf && reference.Type != References.FollowsFrom) { return null; }

            var context = (ILetsTraceSpanContext) reference.Context;
            var type = reference.Type == References.ChildOf ? JaegerReferenceType.CHILD_OF : JaegerReferenceType.FOLLOWS_FROM;
            return new JaegerReference(type, (long)context.TraceId.Low, (long)context.TraceId.High, context.SpanId);
        }

        internal static JaegerLog BuildJaegerLog(LogRecord log)
        {
            return new JaegerLog(log.Timestamp.ToUnixTimeMicroseconds(), ConvertLogToJaegerTags(log));
        }

        internal static List<JaegerTag> ConvertLogToJaegerTags(LogRecord log)
        {
            var tags = new List<JaegerTag>();

            foreach(var field in log.Fields)
            {
                field.Marshal(tags);
            }

            return tags;
        }

        internal static JaegerProcess BuildJaegerProcessThrift(ILetsTraceTracer tracer)
        {
            return new JaegerProcess(tracer.ServiceName)
            {
                Tags = BuildJaegerTags(tracer.Tags)
            };
        }

        public void Dispose()
        {
            CloseAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            var count = _buffer.Count;

            try
            {
                await SendAsync(_buffer, cancellationToken);
            }
            catch (Exception e)
            {
                throw new SenderException("Failed to flush spans.", e, count);
            }
            finally
            {
                _buffer.Clear();
            }

            return count;
        }

        protected abstract Task SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken);

        public virtual Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            return FlushAsync(cancellationToken);
        }
    }
}