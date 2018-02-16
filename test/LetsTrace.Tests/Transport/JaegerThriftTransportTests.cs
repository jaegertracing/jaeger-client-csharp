using System;
using System.Collections.Generic;
using LetsTrace.Transport.Jaeger;
using NSubstitute;
using OpenTracing;
using Xunit;
using JaegerReferenceType = Jaeger.Thrift.SpanRefType;
using JaegerTagType = Jaeger.Thrift.TagType;

namespace LetsTrace.Tests
{
    public class JaegerThriftTransportTests
    {
        [Fact]
        public void JaegerHTTPTransport_BuildJaegerReference_BuildsChildOfCorrectly()
        {
            var refType = References.ChildOf;
            var context = Substitute.For<ILetsTraceSpanContext>();
            var traceId = new TraceId { Low = 152387, High = 4587234 };
            var spanId = new SpanId(3087);

            context.TraceId.Returns(traceId);
            context.SpanId.Returns(spanId);
            
            var reference = new Reference(refType, context);

            var converted = JaegerThriftTransport.BuildJaegerReference(reference);

            Assert.Equal(JaegerReferenceType.CHILD_OF, converted.RefType);
            Assert.Equal((long)traceId.Low, converted.TraceIdLow);
            Assert.Equal((long)traceId.High, converted.TraceIdHigh);
            Assert.Equal(spanId, converted.SpanId);
        }

        [Fact]
        public void JaegerHTTPTransport_BuildJaegerReference_BuildsFollowsFromCorrectly()
        {
            var refType = References.FollowsFrom;
            var context = Substitute.For<ILetsTraceSpanContext>();
            var traceId = new TraceId { Low = 98246, High = 477924576 };
            var spanId = new SpanId(846);

            context.TraceId.Returns(traceId);
            context.SpanId.Returns(spanId);
            
            var reference = new Reference(refType, context);

            var converted = JaegerThriftTransport.BuildJaegerReference(reference);

            Assert.Equal(JaegerReferenceType.FOLLOWS_FROM, converted.RefType);
            Assert.Equal((long)traceId.Low, converted.TraceIdLow);
            Assert.Equal((long)traceId.High, converted.TraceIdHigh);
            Assert.Equal(spanId, converted.SpanId);
        }

        [Fact]
        public void JaegerHTTPTransport_BuildJaegerReference_ShouldReturnNullIfNoJaegerReferenceTypeMatches()
        {
            var refType = "sibling";
            var context = Substitute.For<ILetsTraceSpanContext>();
            var traceId = new TraceId { Low = 98246, High = 477924576 };
            var spanId = new SpanId(846);

            context.TraceId.Returns(traceId);
            context.SpanId.Returns(spanId);
            
            var reference = new Reference(refType, context);

            var converted = JaegerThriftTransport.BuildJaegerReference(reference);

            Assert.Null(converted);
        }

        [Fact]
        public void JaegerHTTPTransport_BuildJaegerLog()
        {
            var timestamp = DateTimeOffset.Parse("2/16/18 11:33:29 AM +00:00");
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

            var converted = JaegerThriftTransport.BuildJaegerLog(log);

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
            Assert.Equal((long)uint16Field.Value, converted.Fields[3].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[4].VType);
            Assert.Equal(uint32Field.Key, converted.Fields[4].Key);
            Assert.Equal((long)uint32Field.Value, converted.Fields[4].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[5].VType);
            Assert.Equal(uint64Field.Key, converted.Fields[5].Key);
            Assert.Equal((long)uint64Field.Value, converted.Fields[5].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[6].VType);
            Assert.Equal(int16Field.Key, converted.Fields[6].Key);
            Assert.Equal((long)int16Field.Value, converted.Fields[6].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[7].VType);
            Assert.Equal(int32Field.Key, converted.Fields[7].Key);
            Assert.Equal((long)int32Field.Value, converted.Fields[7].VLong);

            Assert.Equal(JaegerTagType.LONG, converted.Fields[8].VType);
            Assert.Equal(int64Field.Key, converted.Fields[8].Key);
            Assert.Equal((long)int64Field.Value, converted.Fields[8].VLong);

            Assert.Equal(JaegerTagType.STRING, converted.Fields[9].VType);
            Assert.Equal(stringField.Key, converted.Fields[9].Key);
            Assert.Equal(stringField.Value, converted.Fields[9].VStr);

            Assert.Equal(JaegerTagType.BINARY, converted.Fields[10].VType);
            Assert.Equal(binaryField.Key, converted.Fields[10].Key);
            Assert.Equal(binaryField.Value, converted.Fields[10].VBinary);
        }
    }
}
