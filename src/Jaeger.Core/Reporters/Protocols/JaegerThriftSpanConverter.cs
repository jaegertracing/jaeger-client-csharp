using System;
using System.Collections.Generic;
using System.Globalization;
using Jaeger.Core.Util;
using OpenTracing;
using ThriftSpan = Jaeger.Thrift.Span;
using ThriftTag = Jaeger.Thrift.Tag;
using ThriftReference = Jaeger.Thrift.SpanRef;
using ThriftReferenceType = Jaeger.Thrift.SpanRefType;
using ThriftLog = Jaeger.Thrift.Log;
using ThriftTagType = Jaeger.Thrift.TagType;

namespace Jaeger.Core.Reporters.Protocols
{
    public static class JaegerThriftSpanConverter
    {
        public static ThriftSpan ConvertSpan(Span span)
        {
            var context = span.Context;
            var startTime = span.StartTimestampUtc.ToUnixTimeMicroseconds();
            var duration = (span.FinishTimestampUtc?.ToUnixTimeMicroseconds() - startTime) ?? 0;

            var references = span.GetReferences();
            bool oneChildOfParent = references.Count == 1 && string.Equals(References.ChildOf, references[0].Type, StringComparison.Ordinal);

            var thriftSpan = new ThriftSpan(
                context.TraceId.Low,
                context.TraceId.High,
                context.SpanId,
                oneChildOfParent ? context.ParentId : 0L,
                span.OperationName,
                (byte)context.Flags,
                startTime,
                duration
            )
            {
                References = oneChildOfParent ? new List<ThriftReference>() : BuildReferences(references),
                Tags = BuildTags(span.GetTags()),
                Logs = BuildLogs(span.GetLogs()),
            };

            return thriftSpan;
        }

        internal static List<ThriftReference> BuildReferences(IReadOnlyList<Reference> references)
        {
            List<ThriftReference> thriftReferences = new List<ThriftReference>(references.Count);
            foreach (var reference in references)
            {
                ThriftReferenceType thriftRefType = string.Equals(References.ChildOf, reference.Type, StringComparison.Ordinal)
                    ? ThriftReferenceType.CHILD_OF
                    : ThriftReferenceType.FOLLOWS_FROM;

                thriftReferences.Add(new ThriftReference(
                    thriftRefType,
                    reference.Context.TraceId.Low,
                    reference.Context.TraceId.High,
                    reference.Context.SpanId));
            }

            return thriftReferences;
        }

        private static List<ThriftLog> BuildLogs(IEnumerable<LogData> logs)
        {
            List<ThriftLog> thriftLogs = new List<ThriftLog>();
            if (logs != null)
            {
                foreach (LogData logData in logs)
                {
                    ThriftLog thriftLog = new ThriftLog();
                    thriftLog.Timestamp = logData.TimestampUtc.ToUnixTimeMicroseconds();
                    if (logData.Fields != null)
                    {
                        thriftLog.Fields = BuildTags(logData.Fields);
                    }
                    else
                    {
                        List<ThriftTag> thriftTags = new List<ThriftTag>();
                        if (logData.Message != null)
                        {
                            thriftTags.Add(BuildTag("event", logData.Message));
                        }
                        thriftLog.Fields = thriftTags;
                    }
                    thriftLogs.Add(thriftLog);
                }
            }
            return thriftLogs;
        }

        internal static List<ThriftTag> BuildTags(IEnumerable<KeyValuePair<string, object>> tags)
        {
            var thriftTags = new List<ThriftTag>();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    thriftTags.Add(BuildTag(tag.Key, tag.Value));
                }
            }
            return thriftTags;
        }

        internal static ThriftTag BuildTag(string key, object value)
        {
            switch (value)
            {
                case byte[] val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.BINARY, VBinary = val };
                case double val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.DOUBLE, VDouble = val };
                case decimal val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.DOUBLE, VDouble = (double)val };
                case float val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.DOUBLE, VDouble = val };
                case bool val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.BOOL, VBool = val };
                case ushort val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.LONG, VLong = val };
                case uint val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.LONG, VLong = val };
                case ulong val when val <= long.MaxValue:
                    return new ThriftTag { Key = key, VType = ThriftTagType.LONG, VLong = (long)val };
                case short val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.LONG, VLong = val };
                case int val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.LONG, VLong = val };
                case long val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.LONG, VLong = val };
                case string val:
                    return new ThriftTag { Key = key, VType = ThriftTagType.STRING, VStr = val };
                default:
                    return new ThriftTag { Key = key, VType = ThriftTagType.STRING, VStr = Convert.ToString(value, CultureInfo.InvariantCulture) };
            }
        }
    }
}