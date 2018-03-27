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
            var doubleField = new Field<double> { Key = "doubleField", Value = 1.1 };
            var decimalField = new Field<decimal> { Key = "decimalField", Value = 5.5m };
            var boolField = new Field<bool> { Key = "boolField", Value = true };
            var uint16Field = new Field<UInt16> { Key = "uint16Field", Value = 5 };
            var uint32Field = new Field<UInt32> { Key = "uint32Field", Value = 12 };
            var uint64Field = new Field<UInt64> { Key = "uint64Field", Value = 549 };
            var int16Field = new Field<Int16> { Key = "int16Field", Value = 95 };
            var int32Field = new Field<Int32> { Key = "int32Field", Value = 346 };
            var int64Field = new Field<Int64> { Key = "int64Field", Value = 1942 };

            var stringField = new Field<string> { Key = "stringField", Value = "stringValue" };
            var binaryField = new Field<byte[]> { Key = "binaryField", Value = new byte[7] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 } };

            var fields = new List<Field> {
                doubleField,
                decimalField,
                boolField,
                uint16Field,
                uint32Field,
                uint64Field,
                int16Field,
                int32Field,
                int64Field,
                stringField,
                binaryField
            };
            var log = new LogRecord(timestamp, fields);

            var converted = JaegerThriftSerialization.BuildJaegerLog(log);

            Assert.Equal(1518780809000000, converted.Timestamp);

            Assert.Equal(JaegerTagType.DOUBLE, converted.Fields[0].VType);
            Assert.Equal(doubleField.Key, converted.Fields[0].Key);
            Assert.Equal(doubleField.Value, converted.Fields[0].VDouble);

            Assert.Equal(JaegerTagType.DOUBLE, converted.Fields[1].VType);
            Assert.Equal(decimalField.Key, converted.Fields[1].Key);
            Assert.Equal((double)decimalField.Value, converted.Fields[1].VDouble);

            Assert.Equal(JaegerTagType.BOOL, converted.Fields[2].VType);
            Assert.Equal(boolField.Key, converted.Fields[2].Key);
            Assert.Equal(boolField.Value, converted.Fields[2].VBool);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[3].VType);
            Assert.Equal(uint16Field.Key, converted.Fields[3].Key);
            Assert.Equal(uint16Field.Value, converted.Fields[3].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[4].VType);
            Assert.Equal(uint32Field.Key, converted.Fields[4].Key);
            Assert.Equal(uint32Field.Value, converted.Fields[4].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[5].VType);
            Assert.Equal(uint64Field.Key, converted.Fields[5].Key);
            Assert.Equal((long)uint64Field.Value, converted.Fields[5].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[6].VType);
            Assert.Equal(int16Field.Key, converted.Fields[6].Key);
            Assert.Equal(int16Field.Value, converted.Fields[6].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[7].VType);
            Assert.Equal(int32Field.Key, converted.Fields[7].Key);
            Assert.Equal(int32Field.Value, converted.Fields[7].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[8].VType);
            Assert.Equal(int64Field.Key, converted.Fields[8].Key);
            Assert.Equal(int64Field.Value, converted.Fields[8].VLong);

            Assert.Equal(JaegerTagType.STRING, converted.Fields[9].VType);
            Assert.Equal(stringField.Key, converted.Fields[9].Key);
            Assert.Equal(stringField.Value, converted.Fields[9].VStr);

            Assert.Equal(JaegerTagType.BINARY, converted.Fields[10].VType);
            Assert.Equal(binaryField.Key, converted.Fields[10].Key);
            Assert.Equal(binaryField.Value, converted.Fields[10].VBinary);
        }

        [Fact]
        public void BuildJeagerProcessThrift()
        {
            var doubleField = new Field<double> { Key = "doubleField", Value = 1.1 };
            var int64Field = new Field<Int64> { Key = "int64Field", Value = 1942 };
            var stringField = new Field<string> { Key = "stringField", Value = "stringValue" };

            var serialization = new JaegerThriftSerialization();
            var tracer = Substitute.For<ILetsTraceTracer>();
            tracer.ServiceName.Returns("testingService");
            var tracerTags = new Dictionary<string, Field>
            {
                { "doubleTag", doubleField },
                { "int64Tag", int64Field },
                { "stringTag", stringField }
            };
            tracer.Tags.Returns(tracerTags);

            var process = serialization.BuildJaegerProcessThrift(tracer);

            Assert.Equal(tracer.ServiceName, process.ServiceName);
            Assert.Equal(3, process.Tags.Count);
            Assert.Equal("doubleTag", process.Tags[0].Key);
            Assert.Equal(doubleField.Value, process.Tags[0].VDouble);
            Assert.Equal("int64Tag", process.Tags[1].Key);
            Assert.Equal(int64Field.Value, process.Tags[1].VLong);
            Assert.Equal("stringTag", process.Tags[2].Key);
            Assert.Equal(stringField.Value, process.Tags[2].VStr);
        }

        [Fact]
        public void BuildJeagerThriftSpan()
        {
            var doubleField = new Field<double> { Key = "doubleField", Value = 1.1 };
            var decimalField = new Field<decimal> { Key = "decimalField", Value = 5.5m };
            var int64Field = new Field<Int64> { Key = "int64Field", Value = 1942 };
            var stringField = new Field<string> { Key = "stringField", Value = "stringValue" };
            var tracerTags = new Dictionary<string, Field>
            {
                { "doubleTag", doubleField },
                { "int64Tag", int64Field },
                { "stringTag", stringField }
            };
            var logTimestamp = new DateTime(2018, 2, 16, 11, 33, 29, DateTimeKind.Utc);
            var logFields1 = new List<Field> {
                doubleField
            };
            var logFields2 = new List<Field> {
                decimalField
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
            Assert.Equal("doubleTag", jSpan.Tags[0].Key);
            Assert.Equal(doubleField.Value, jSpan.Tags[0].VDouble);
            Assert.Equal("int64Tag", jSpan.Tags[1].Key);
            Assert.Equal(int64Field.Value, jSpan.Tags[1].VLong);
            Assert.Equal("stringTag", jSpan.Tags[2].Key);
            Assert.Equal(stringField.Value, jSpan.Tags[2].VStr);

            // logs
            Assert.Equal(JaegerTagType.DOUBLE, jSpan.Logs[0].Fields[0].VType);
            Assert.Equal(doubleField.Key, jSpan.Logs[0].Fields[0].Key);
            Assert.Equal(doubleField.Value, jSpan.Logs[0].Fields[0].VDouble);

            Assert.Equal(JaegerTagType.DOUBLE, jSpan.Logs[1].Fields[0].VType);
            Assert.Equal(decimalField.Key, jSpan.Logs[1].Fields[0].Key);
            Assert.Equal((double)decimalField.Value, jSpan.Logs[1].Fields[0].VDouble);

            // references
            Assert.Single(jSpan.References);
            Assert.Equal(JaegerReferenceType.CHILD_OF, jSpan.References[0].RefType);
            Assert.Equal((long)traceId.Low, jSpan.References[0].TraceIdLow);
            Assert.Equal((long)traceId.High, jSpan.References[0].TraceIdHigh);
            Assert.Equal(spanId, jSpan.References[0].SpanId);
        }
    }
}
