using System;
using System.Collections.Generic;
using LetsTrace.Jaeger.Serialization;
using NSubstitute;
using OpenTracing;
using Xunit;
using JaegerReferenceType = Jaeger.Thrift.SpanRefType;
using JaegerTagType = Jaeger.Thrift.TagType;

namespace LetsTrace.Jaeger.Tests.Serialization
{
    public class JaegerThriftSerializationTests
    {
        [Fact]
        public void BuildJaegerReference_BuildsChildOfCorrectly()
        {
            var refType = References.ChildOf;
            var context = Substitute.For<ILetsTraceSpanContext>();
            var traceId = new TraceId(152387, 4587234);
            var spanId = new SpanId(3087);

            context.TraceId.Returns(traceId);
            context.SpanId.Returns(spanId);

            var reference = new Reference(refType, context);

            var converted = JaegerThriftSerialization.BuildJaegerReference(reference);

            Assert.Equal(JaegerReferenceType.CHILD_OF, converted.RefType);
            Assert.Equal((long)traceId.Low, converted.TraceIdLow);
            Assert.Equal((long)traceId.High, converted.TraceIdHigh);
            Assert.Equal(spanId, converted.SpanId);
        }

        [Fact]
        public void BuildJaegerReference_BuildsFollowsFromCorrectly()
        {
            var refType = References.FollowsFrom;
            var context = Substitute.For<ILetsTraceSpanContext>();
            var traceId = new TraceId(98246, 477924576);
            var spanId = new SpanId(846);

            context.TraceId.Returns(traceId);
            context.SpanId.Returns(spanId);

            var reference = new Reference(refType, context);

            var converted = JaegerThriftSerialization.BuildJaegerReference(reference);

            Assert.Equal(JaegerReferenceType.FOLLOWS_FROM, converted.RefType);
            Assert.Equal((long)traceId.Low, converted.TraceIdLow);
            Assert.Equal((long)traceId.High, converted.TraceIdHigh);
            Assert.Equal(spanId, converted.SpanId);
        }

        [Fact]
        public void BuildJaegerReference_ShouldReturnNullIfNoJaegerReferenceTypeMatches()
        {
            var refType = "sibling";
            var context = Substitute.For<ILetsTraceSpanContext>();
            var traceId = new TraceId(98246, 477924576);
            var spanId = new SpanId(846);

            context.TraceId.Returns(traceId);
            context.SpanId.Returns(spanId);

            var reference = new Reference(refType, context);

            var converted = JaegerThriftSerialization.BuildJaegerReference(reference);

            Assert.Null(converted);
        }

        [Fact]
        public void BuildJaegerLog()
        {
            var timestamp = new DateTime(2018, 2, 16, 11, 33, 29, DateTimeKind.Utc);
            var doubleFieldKey = "doubleField";
            var decimalFieldKey = "decimalField";
            var boolFieldKey = "boolField";
            var ushortFieldKey = "uint16Field";
            var uintFieldKey = "uint32Field";
            var ulongFieldKey = "uint64Field";
            var shortFieldKey = "int16Field";
            var intFieldKey = "int32Field";
            var longFieldKey = "int64Field";
            var stringFieldKey = "stringField";
            var binaryFieldKey = "binaryField";

            var fields = new Dictionary<string, object> {
                { doubleFieldKey, 1.1 },
                { decimalFieldKey, 5.5m },
                { boolFieldKey, true },
                { ushortFieldKey, (ushort)5 },
                { uintFieldKey, (uint)12 },
                { ulongFieldKey, ulong.MaxValue },
                { shortFieldKey, (short)95 },
                { intFieldKey, 346 },
                { longFieldKey, (long)1942 },
                { stringFieldKey, "stringValue" },
                { binaryFieldKey, new byte[7] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 } }
            };
            var log = new LogRecord(timestamp, fields);

            var converted = JaegerThriftSerialization.BuildJaegerLog(log);

            Assert.Equal(1518780809000000, converted.Timestamp);

            Assert.Equal(JaegerTagType.DOUBLE, converted.Fields[0].VType);
            Assert.Equal(doubleFieldKey, converted.Fields[0].Key);
            Assert.Equal(fields[doubleFieldKey], converted.Fields[0].VDouble);

            Assert.Equal(JaegerTagType.DOUBLE, converted.Fields[1].VType);
            Assert.Equal(decimalFieldKey, converted.Fields[1].Key);
            Assert.Equal(Convert.ToDouble(fields[decimalFieldKey]), converted.Fields[1].VDouble);

            Assert.Equal(JaegerTagType.BOOL, converted.Fields[2].VType);
            Assert.Equal(boolFieldKey, converted.Fields[2].Key);
            Assert.Equal(fields[boolFieldKey], converted.Fields[2].VBool);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[3].VType);
            Assert.Equal(ushortFieldKey, converted.Fields[3].Key);
            Assert.Equal(Convert.ToInt64(fields[ushortFieldKey]), converted.Fields[3].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[4].VType);
            Assert.Equal(uintFieldKey, converted.Fields[4].Key);
            Assert.Equal(Convert.ToInt64(fields[uintFieldKey]), converted.Fields[4].VLong);

            Assert.Equal(JaegerTagType.STRING, converted.Fields[5].VType);
            Assert.Equal(ulongFieldKey, converted.Fields[5].Key);
            Assert.Equal($"Ulong: {fields[ulongFieldKey]}", converted.Fields[5].VStr);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[6].VType);
            Assert.Equal(shortFieldKey, converted.Fields[6].Key);
            Assert.Equal(Convert.ToInt64(fields[shortFieldKey]), converted.Fields[6].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[7].VType);
            Assert.Equal(intFieldKey, converted.Fields[7].Key);
            Assert.Equal(Convert.ToInt64(fields[intFieldKey]), converted.Fields[7].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[8].VType);
            Assert.Equal(longFieldKey, converted.Fields[8].Key);
            Assert.Equal(fields[longFieldKey], converted.Fields[8].VLong);

            Assert.Equal(JaegerTagType.STRING, converted.Fields[9].VType);
            Assert.Equal(stringFieldKey, converted.Fields[9].Key);
            Assert.Equal(fields[stringFieldKey], converted.Fields[9].VStr);

            Assert.Equal(JaegerTagType.BINARY, converted.Fields[10].VType);
            Assert.Equal(binaryFieldKey, converted.Fields[10].Key);
            Assert.Equal(fields[binaryFieldKey], converted.Fields[10].VBinary);
        }

        [Fact]
        public void BuildJeagerProcessThrift()
        {
            var serialization = new JaegerThriftSerialization();
            var tracer = Substitute.For<ILetsTraceTracer>();
            tracer.ServiceName.Returns("testingService");
            var tracerTags = new Dictionary<string, object>
            {
                { "doubleTag", 1.1 },
                { "intTag", 1942 },
                { "stringTag", "stringValue" }
            };
            tracer.Tags.Returns(tracerTags);

            var process = serialization.BuildJaegerProcessThrift(tracer);

            Assert.Equal(tracer.ServiceName, process.ServiceName);
            Assert.Equal(3, process.Tags.Count);
            Assert.Equal("doubleTag", process.Tags[0].Key);
            Assert.Equal(tracerTags["doubleTag"], process.Tags[0].VDouble);
            Assert.Equal("intTag", process.Tags[1].Key);
            Assert.Equal(Convert.ToInt64(tracerTags["intTag"]), process.Tags[1].VLong);
            Assert.Equal("stringTag", process.Tags[2].Key);
            Assert.Equal(tracerTags["stringTag"], process.Tags[2].VStr);
        }

        [Fact]
        public void BuildJeagerThriftSpan()
        {
            var doubleTagKey = "doubleTag";
            var doubleLogKey = "doubleLog";
            var decimalLogKey = "decimalLog";
            var int64TagKey = "int64Tag";
            var stringTagKey = "stringTag";
            var tracerTags = new Dictionary<string, object>
            {
                { doubleTagKey, 1.1 },
                { int64TagKey, (Int64)1942 },
                { stringTagKey, "stringValue" }
            };
            var logTimestamp = new DateTime(2018, 2, 16, 11, 33, 29, DateTimeKind.Utc);
            var logFields1 = new Dictionary<string, object>
            {
                { doubleLogKey, 1.3 }
            };
            var logFields2 = new Dictionary<string, object>
            {
                { decimalLogKey, 5.5m }
            };
            var logs = new List<LogRecord>
            {
                new LogRecord(logTimestamp, logFields1),
                new LogRecord(logTimestamp, logFields2)
            };

            var serialization = new JaegerThriftSerialization();
            var span = Substitute.For<ILetsTraceSpan>();
            var traceId = new TraceId(10, 2);
            var spanId = new SpanId(15);
            var parentId = new SpanId(82);
            var context = new SpanContext(traceId, spanId, parentId);
            span.Context.Returns(context);
            var op = "op, yo";
            span.OperationName.Returns(op);
            span.Tags.Returns(tracerTags);
            span.Logs.Returns(logs);
            var startTimestamp = new DateTime(2018, 2, 16, 11, 33, 28, DateTimeKind.Utc);
            var finishTimestamp = new DateTime(2018, 2, 16, 11, 33, 30, DateTimeKind.Utc);
            span.StartTimestampUtc.Returns(startTimestamp);
            span.FinishTimestampUtc.Returns(finishTimestamp);

            var parentRefType = References.ChildOf;
            var parentContext = Substitute.For<ILetsTraceSpanContext>();
            parentContext.TraceId.Returns(traceId);
            parentContext.SpanId.Returns(parentId);
            var reference = new Reference(parentRefType, context);
            span.References.Returns(new List<Reference> { reference });

            var jSpan = serialization.BuildJaegerThriftSpan(span);

            Assert.Equal((long)traceId.Low, jSpan.TraceIdLow);
            Assert.Equal((long)traceId.High, jSpan.TraceIdHigh);
            Assert.Equal(spanId, jSpan.SpanId);
            Assert.Equal(parentId, jSpan.ParentSpanId);
            Assert.Equal(op, jSpan.OperationName);
            Assert.Equal(0, jSpan.Flags);
            Assert.Equal(1518780808000000, jSpan.StartTime);
            Assert.Equal(2000000, jSpan.Duration);

            // tags
            Assert.Equal(3, jSpan.Tags.Count);
            Assert.Equal(doubleTagKey, jSpan.Tags[0].Key);
            Assert.Equal(1.1, jSpan.Tags[0].VDouble);
            Assert.Equal(int64TagKey, jSpan.Tags[1].Key);
            Assert.Equal(1942, jSpan.Tags[1].VLong);
            Assert.Equal(stringTagKey, jSpan.Tags[2].Key);
            Assert.Equal("stringValue", jSpan.Tags[2].VStr);

            // logs
            Assert.Equal(JaegerTagType.DOUBLE, jSpan.Logs[0].Fields[0].VType);
            Assert.Equal(doubleLogKey, jSpan.Logs[0].Fields[0].Key);
            Assert.Equal(1.3, jSpan.Logs[0].Fields[0].VDouble);

            Assert.Equal(JaegerTagType.DOUBLE, jSpan.Logs[1].Fields[0].VType);
            Assert.Equal(decimalLogKey, jSpan.Logs[1].Fields[0].Key);
            Assert.Equal((double)5.5m, jSpan.Logs[1].Fields[0].VDouble);

            // references
            Assert.Single(jSpan.References);
            Assert.Equal(JaegerReferenceType.CHILD_OF, jSpan.References[0].RefType);
            Assert.Equal((long)traceId.Low, jSpan.References[0].TraceIdLow);
            Assert.Equal((long)traceId.High, jSpan.References[0].TraceIdHigh);
            Assert.Equal(spanId, jSpan.References[0].SpanId);
        }
    }
}
