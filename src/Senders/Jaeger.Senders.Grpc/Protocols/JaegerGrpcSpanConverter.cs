using System;
using System.Collections.Generic;
using System.Globalization;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using OpenTracing;
using GrpcSpan = Jaeger.ApiV2.Span;
using GrpcTag = Jaeger.ApiV2.KeyValue;
using GrpcReference = Jaeger.ApiV2.SpanRef;
using GrpcReferenceType = Jaeger.ApiV2.SpanRefType;
using GrpcLog = Jaeger.ApiV2.Log;
using GrpcTagType = Jaeger.ApiV2.ValueType;

namespace Jaeger.Senders.Grpc.Protocols
{
    public static class JaegerGrpcSpanConverter
    {
        public static GrpcSpan ConvertSpan(Span span)
        {
            var context = span.Context;
            var startTime = span.StartTimestampUtc;
            var duration = (span.FinishTimestampUtc ?? startTime) - startTime;

            var references = span.GetReferences();
            var oneChildOfParent = references.Count == 1 && string.Equals(References.ChildOf, references[0].Type, StringComparison.Ordinal);

            var grpcSpan = new GrpcSpan
            {
                TraceId = ByteString.CopyFrom(context.TraceId.ToByteArray()),
                SpanId = ByteString.CopyFrom(context.SpanId.ToByteArray()),
                OperationName = span.OperationName,
                Flags = (uint)context.Flags,
                StartTime = Timestamp.FromDateTime(startTime),
                Duration = Duration.FromTimeSpan(duration),
                References = { oneChildOfParent ? new List<GrpcReference>() : BuildReferences(references) },
                Tags = { BuildTags(span.GetTags()) },
                Logs = { BuildLogs(span.GetLogs()) }
            };

            return grpcSpan;
        }

        internal static List<GrpcReference> BuildReferences(IReadOnlyList<Reference> references)
        {
            List<GrpcReference> grpcReferences = new List<GrpcReference>(references.Count);
            foreach (var reference in references)
            {
                GrpcReferenceType grpcRefType = string.Equals(References.ChildOf, reference.Type, StringComparison.Ordinal)
                    ? GrpcReferenceType.ChildOf
                    : GrpcReferenceType.FollowsFrom;

                grpcReferences.Add(new GrpcReference
                {
                    RefType = grpcRefType,
                    TraceId = ByteString.CopyFrom(reference.Context.TraceId.ToByteArray()),
                    SpanId = ByteString.CopyFrom(reference.Context.SpanId.ToByteArray())
                });
            }

            return grpcReferences;
        }

        private static List<GrpcLog> BuildLogs(IEnumerable<LogData> logs)
        {
            List<GrpcLog> grpcLogs = new List<GrpcLog>();
            if (logs != null)
            {
                foreach (LogData logData in logs)
                {
                    GrpcLog grpcLog = new GrpcLog
                    {
                        Timestamp = Timestamp.FromDateTime(logData.TimestampUtc)
                    };

                    if (logData.Fields != null)
                    {
                        grpcLog.Fields.AddRange(BuildTags(logData.Fields));
                    }
                    else if (logData.Message != null)
                    {
                        grpcLog.Fields.Add(BuildTag(LogFields.Event, logData.Message));
                    }

                    grpcLogs.Add(grpcLog);
                }
            }
            return grpcLogs;
        }

        internal static List<GrpcTag> BuildTags(IEnumerable<KeyValuePair<string, object>> tags)
        {
            var grpcTags = new List<GrpcTag>();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    grpcTags.Add(BuildTag(tag.Key, tag.Value));
                }
            }
            return grpcTags;
        }

        internal static GrpcTag BuildTag(string key, object value)
        {
            switch (value)
            {
                case byte[] val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Binary, VBinary = ByteString.CopyFrom(val) };
                case double val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Float64, VFloat64 = val };
                case decimal val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Float64, VFloat64 = (double)val };
                case float val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Float64, VFloat64 = val };
                case bool val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Bool, VBool = val };
                case ushort val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Int64, VInt64 = val };
                case uint val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Int64, VInt64 = val };
                case ulong val when val <= long.MaxValue:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Int64, VInt64 = (long)val };
                case short val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Int64, VInt64 = val };
                case int val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Int64, VInt64 = val };
                case long val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.Int64, VInt64 = val };
                case string val:
                    return new GrpcTag { Key = key, VType = GrpcTagType.String, VStr = val };
                // TODO: We might want to support stringification of lists and objects.
                default:
                    return new GrpcTag { Key = key, VType = GrpcTagType.String, VStr = Convert.ToString(value, CultureInfo.InvariantCulture) };
            }
        }
    }
}