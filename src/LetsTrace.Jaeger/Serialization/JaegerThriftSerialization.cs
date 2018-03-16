using System.Linq;
using System.Collections.Generic;
using OpenTracing;

using LetsTrace.Util;
using LetsTrace.Jaeger.Transport;

using JaegerSpan = Jaeger.Thrift.Span;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerTag = Jaeger.Thrift.Tag;
using JaegerReference = Jaeger.Thrift.SpanRef;
using JaegerReferenceType = Jaeger.Thrift.SpanRefType;
using JaegerLog = Jaeger.Thrift.Log;

namespace LetsTrace.Jaeger.Serialization
{
    public class JaegerThriftSerialization : ISerialization
    {

        public JaegerSpan BuildJaegerThriftSpan(ILetsTraceSpan span)
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

        public JaegerProcess BuildJaegerProcessThrift(ILetsTraceTracer tracer)
        {
            return new JaegerProcess(tracer.ServiceName)
            {
                Tags = BuildJaegerTags(tracer.Tags)
            };
        }

        public static List<JaegerTag> BuildJaegerTags(IDictionary<string, Field> inTags)
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
        public static JaegerReference BuildJaegerReference(Reference reference)
        {
            if (reference.Type != References.ChildOf && reference.Type != References.FollowsFrom) { return null; }

            var context = (ILetsTraceSpanContext)reference.Context;
            var type = reference.Type == References.ChildOf ? JaegerReferenceType.CHILD_OF : JaegerReferenceType.FOLLOWS_FROM;
            return new JaegerReference(type, (long)context.TraceId.Low, (long)context.TraceId.High, context.SpanId);
        }

        public static JaegerLog BuildJaegerLog(LogRecord log)
        {
            return new JaegerLog(log.Timestamp.ToUnixTimeMicroseconds(), ConvertLogToJaegerTags(log));
        }

        public static List<JaegerTag> ConvertLogToJaegerTags(LogRecord log)
        {
            var tags = new List<JaegerTag>();

            foreach (var field in log.Fields)
            {
                field.Marshal(tags);
            }

            return tags;
        }
    }
}
