using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using LetsTrace.Util;
using Thrift.Protocols;
using Thrift.Transports;
using OpenTracing;
using Thrift.Transports.Client;
using System.Threading;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
        private List<JaegerSpan> _buffer = new List<JaegerSpan>(); // TODO: look into making this thread safe
        private int _bufferSize = 10;
        protected JaegerProcess _process = null;
        protected static readonly HttpClient HttpClient = new HttpClient();

        public JaegerThriftTransport(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public async Task<int> Append(ILetsTraceSpan span)
        {
            if (_process == null) {
                _process = BuildJaegerProcessThrift(span.Tracer);
            }
            var jaegerSpan = BuildJaegerThriftSpan(span);

            _buffer.Add(jaegerSpan);

            if (_buffer.Count > _bufferSize) {
                return await Flush();
            }

            return 0;
        }

        internal static JaegerSpan BuildJaegerThriftSpan(ILetsTraceSpan span)
        {
            var context = (ILetsTraceSpanContext) span.Context;
            var startTime = span.StartTimestamp.ToUnixTimeMicroseconds();
            var duration = span.FinishTimestamp == null ?
                0
                : span.FinishTimestamp.GetValueOrDefault().ToUnixTimeMicroseconds() - span.StartTimestamp.ToUnixTimeMicroseconds();

            var jaegerSpan = new JaegerSpan(
                (long)context.TraceId.Low, 
                (long)context.TraceId.High,
                context.SpanId,
                context.ParentId,
                span.OperationName,
                0,
                startTime,
                duration
            );
            span.References.Select(r => r);
            foreach(var tag in span.Tags)
            {
                tag.Value.Key = tag.Key;
                tag.Value.Marshal(jaegerSpan.Tags);
            }
            jaegerSpan.Logs = span.Logs.Select(BuildJaegerLog).ToList();
            jaegerSpan.References = span.References.Select(BuildJaegerReference).Where(r => r != null).ToList();

            return jaegerSpan;
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
            return new JaegerProcess(tracer.ServiceName);
        }

        public void Dispose()
        {
            Flush().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<int> Flush()
        {
            var count = _buffer.Count;

            await Send(_buffer);

            _buffer.Clear();

            return count;
        }

        public abstract Task Send(List<JaegerSpan> spans);
    }
}