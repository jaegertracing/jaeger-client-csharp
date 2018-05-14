using System;
using System.Collections.Generic;
using Jaeger.Core.Reporters;
using Jaeger.Core.Reporters.Protocols;
using Jaeger.Core.Samplers;
using OpenTracing;
using Xunit;
using ThriftLog = Jaeger.Thrift.Log;
using ThriftReference = Jaeger.Thrift.SpanRef;
using ThriftReferenceType = Jaeger.Thrift.SpanRefType;
using ThriftSpan = Jaeger.Thrift.Span;
using ThriftTag = Jaeger.Thrift.Tag;
using ThriftTagType = Jaeger.Thrift.TagType;

namespace Jaeger.Core.Tests.Reporters.Protocols
{
    public class JaegerThriftSpanConverterTest
    {
        private static readonly ThriftReferenceComparer _thriftReferenceComparer = new ThriftReferenceComparer();

        private readonly Tracer _tracer;

        public JaegerThriftSpanConverterTest()
        {
            _tracer = new Tracer.Builder("test-service-name")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .Build();
        }

        public static List<object[]> DataProviderBuildTag() => new List<object[]>
        {
            new object[] { "value", ThriftTagType.STRING, "value" },
            new object[] { (long) 1, ThriftTagType.LONG, (long) 1 },
            new object[] { 1, ThriftTagType.LONG, (long) 1 },
            new object[] { (short) 1, ThriftTagType.LONG, (long) 1 },
            new object[] { (double) 1, ThriftTagType.DOUBLE, (double) 1 },
            new object[] { (float) 1, ThriftTagType.DOUBLE, (double) 1 },
            new object[] { (byte) 1, ThriftTagType.STRING, "1" },
            new object[] { true, ThriftTagType.BOOL, true },
            // TODO C# doesn't use values for ToString() // new object[] { new List<string> { "hello" }, ThriftTagType.STRING, "[hello]" },
            new object[] { new List<string> { "hello" }, ThriftTagType.STRING, "System.Collections.Generic.List`1[System.String]" }
        };

        [Theory]
        [MemberData(nameof(DataProviderBuildTag))]
        public void TestBuildTag(object tagValue, ThriftTagType tagType, object expected)
        {
            ThriftTag tag = JaegerThriftSpanConverter.BuildTag("key", tagValue);
            Assert.Equal(tagType, tag.VType);
            Assert.Equal("key", tag.Key);
            switch (tagType)
            {
                case ThriftTagType.BOOL:
                    Assert.Equal(expected, tag.VBool);
                    break;
                case ThriftTagType.LONG:
                    Assert.Equal(expected, tag.VLong);
                    break;
                case ThriftTagType.DOUBLE:
                    Assert.Equal(expected, tag.VDouble);
                    break;
                case ThriftTagType.BINARY:
                    break;
                case ThriftTagType.STRING:
                default:
                    Assert.Equal(expected, tag.VStr);
                    break;
            }
        }

        [Fact]
        public void TestBuildTags()
        {
            var tags = new Dictionary<string, object> { { "key", "value" } };

            List<ThriftTag> thriftTags = JaegerThriftSpanConverter.BuildTags(tags);
            Assert.NotNull(thriftTags);
            Assert.Single(thriftTags);
            Assert.Equal("key", thriftTags[0].Key);
            Assert.Equal("value", thriftTags[0].VStr);
            Assert.Equal(ThriftTagType.STRING, thriftTags[0].VType);
        }

        [Fact]
        public void TestConvertSpan()
        {
            var logTimestamp = new DateTimeOffset(2018, 4, 13, 10, 30, 0, TimeSpan.Zero);
            var fields = new Dictionary<string, object> { { "k", "v" } };

            Span span = (Span)_tracer.BuildSpan("operation-name").Start();
            span.Log(logTimestamp, fields);
            span.SetBaggageItem("foo", "bar");

            ThriftSpan thriftSpan = JaegerThriftSpanConverter.ConvertSpan(span);

            Assert.Equal("operation-name", thriftSpan.OperationName);
            Assert.Equal(2, thriftSpan.Logs.Count);
            ThriftLog thriftLog = thriftSpan.Logs[0];
            Assert.Equal(logTimestamp.ToUnixTimeMilliseconds() * 1000, thriftLog.Timestamp);
            Assert.Single(thriftLog.Fields);
            ThriftTag thriftTag = thriftLog.Fields[0];
            Assert.Equal("k", thriftTag.Key);
            Assert.Equal("v", thriftTag.VStr);

            // NOTE: In Java, the order is different (event, value, key) because the HashMap algorithm is different.
            thriftLog = thriftSpan.Logs[1];
            Assert.Equal(3, thriftLog.Fields.Count);
            thriftTag = thriftLog.Fields[0];
            Assert.Equal("event", thriftTag.Key);
            Assert.Equal("baggage", thriftTag.VStr);
            thriftTag = thriftLog.Fields[1];
            Assert.Equal("key", thriftTag.Key);
            Assert.Equal("foo", thriftTag.VStr);
            thriftTag = thriftLog.Fields[2];
            Assert.Equal("value", thriftTag.Key);
            Assert.Equal("bar", thriftTag.VStr);
        }

        [Fact]
        public void TestConvertSpanOneReferenceChildOf()
        {
            Span parent = (Span)_tracer.BuildSpan("foo").Start();

            Span child = (Span)_tracer.BuildSpan("foo")
                .AsChildOf(parent)
                .Start();

            ThriftSpan span = JaegerThriftSpanConverter.ConvertSpan(child);

            Assert.Equal((long)child.Context.ParentId, span.ParentSpanId);
            Assert.Empty(span.References);
        }

        [Fact]
        public void TestConvertSpanTwoReferencesChildOf()
        {
            Span parent = (Span)_tracer.BuildSpan("foo").Start();
            Span parent2 = (Span)_tracer.BuildSpan("foo").Start();

            Span child = (Span)_tracer.BuildSpan("foo")
                .AsChildOf(parent)
                .AsChildOf(parent2)
                .Start();

            ThriftSpan span = JaegerThriftSpanConverter.ConvertSpan(child);

            Assert.Equal(0, span.ParentSpanId);
            Assert.Equal(2, span.References.Count);
            Assert.Equal(BuildReference(parent.Context, References.ChildOf), span.References[0], _thriftReferenceComparer);
            Assert.Equal(BuildReference(parent2.Context, References.ChildOf), span.References[1], _thriftReferenceComparer);
        }

        [Fact]
        public void TestConvertSpanMixedReferences()
        {
            Span parent = (Span)_tracer.BuildSpan("foo").Start();
            Span parent2 = (Span)_tracer.BuildSpan("foo").Start();

            Span child = (Span)_tracer.BuildSpan("foo")
                .AddReference(References.FollowsFrom, parent.Context)
                .AsChildOf(parent2)
                .Start();

            ThriftSpan span = JaegerThriftSpanConverter.ConvertSpan(child);

            Assert.Equal(0, span.ParentSpanId);
            Assert.Equal(2, span.References.Count);
            Assert.Equal(BuildReference(parent.Context, References.FollowsFrom), span.References[0], _thriftReferenceComparer);
            Assert.Equal(BuildReference(parent2.Context, References.ChildOf), span.References[1], _thriftReferenceComparer);
        }

        private static ThriftReference BuildReference(SpanContext context, string referenceType)
        {
            return JaegerThriftSpanConverter.BuildReferences(new List<Reference> { new Reference(context, referenceType) }.AsReadOnly())[0];
        }

        private class ThriftReferenceComparer : EqualityComparer<ThriftReference>
        {
            public override bool Equals(ThriftReference x, ThriftReference y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;

                return x.RefType == y.RefType
                    && x.SpanId == y.SpanId
                    && x.TraceIdHigh == y.TraceIdHigh
                    && x.TraceIdLow == y.TraceIdLow;
            }

            public override int GetHashCode(ThriftReference obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}