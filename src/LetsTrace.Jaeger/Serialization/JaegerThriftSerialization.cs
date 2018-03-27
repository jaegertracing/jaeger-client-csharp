using System;
using System.Linq;
using System.Collections.Generic;
using OpenTracing;

using LetsTrace.Util;

using JaegerSpan = Jaeger.Thrift.Span;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerTag = Jaeger.Thrift.Tag;
using JaegerReference = Jaeger.Thrift.SpanRef;
using JaegerReferenceType = Jaeger.Thrift.SpanRefType;
using JaegerLog = Jaeger.Thrift.Log;
using JaegerTagType = Jaeger.Thrift.TagType;

namespace LetsTrace.Jaeger.Serialization
{
    public class JaegerThriftSerialization : ISerialization
    {

        public JaegerSpan BuildJaegerThriftSpan(ILetsTraceSpan span)
        {
            var context = span.Context;
            var startTime = span.StartTimestampUtc.ToUnixTimeMicroseconds();
            var duration = (span.FinishTimestampUtc?.ToUnixTimeMicroseconds() - startTime) ?? 0;

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

        public static List<JaegerTag> BuildJaegerTags(IDictionary<string, object> inTags)
        {
            var tags = new List<JaegerTag>();
            foreach (var tag in inTags)
            {
                AddToJaegerTagList(tag.Key, tag.Value, tags);
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
            return new JaegerLog(log.TimestampUtc.ToUnixTimeMicroseconds(), ConvertLogToJaegerTags(log));
        }

        public static List<JaegerTag> ConvertLogToJaegerTags(LogRecord log)
        {
            var tags = new List<JaegerTag>();

            foreach (var field in log.Fields)
            {
                AddToJaegerTagList(field.Key, field.Value, tags);
            }

            return tags;
        }

        public static void AddToJaegerTagList(string key, object value, List<JaegerTag> tags)
        {
            switch(value)
            {
                case byte[] b:
                    tags.Add(new JaegerTag{ Key = key, VType = JaegerTagType.BINARY, VBinary = b });
                    break;
                case string s:
                    tags.Add(new JaegerTag{ Key = key, VType = JaegerTagType.STRING, VStr = s });
                    break;
                case double d:
                    tags.Add(new JaegerTag{ Key = key, VType = JaegerTagType.DOUBLE, VDouble = d });
                    break;
                case decimal d:
                    tags.Add(new JaegerTag{ Key = key, VType = JaegerTagType.DOUBLE, VDouble = (double)d });
                    break;
                case bool b:
                    tags.Add(new JaegerTag{ Key = key, VType = JaegerTagType.BOOL, VBool = b });
                    break;
                case UInt16 u16:
                    tags.Add(new JaegerTag { Key = key, VType = JaegerTagType.LONG, VLong = u16 });
                    break;
                case UInt32 u32:
                    tags.Add(new JaegerTag { Key = key, VType = JaegerTagType.LONG, VLong = u32 });
                    break;
                case UInt64 u64:
                    tags.Add(new JaegerTag{ Key = key, VType = JaegerTagType.LONG, VLong = (long)u64 });
                    break;
                case Int16 i16:
                    tags.Add(new JaegerTag { Key = key, VType = JaegerTagType.LONG, VLong = i16 });
                    break;
                case Int32 i32:
                    tags.Add(new JaegerTag { Key = key, VType = JaegerTagType.LONG, VLong = i32 });
                    break;
                case Int64 i64:
                    tags.Add(new JaegerTag { Key = key, VType = JaegerTagType.LONG, VLong = i64 });
                    break;
            }
        }
    }
}
