using System;
using System.Collections.Generic;
using Jaeger.Encoders.Grpc.Internal;
using Jaeger.Reporters;
using Jaeger.Samplers;
using OpenTracing;
using Xunit;
using GrpcSpan = Jaeger.ApiV2.Span;
using GrpcTag = Jaeger.ApiV2.KeyValue;
using GrpcReference = Jaeger.ApiV2.SpanRef;
using GrpcLog = Jaeger.ApiV2.Log;
using GrpcTagType = Jaeger.ApiV2.ValueType;

namespace Jaeger.Senders.Grpc.Tests.Reporters.Protocols
{
    public class JaegerGrpcSpanConverterTests
    {
        private readonly Tracer _tracer;

        public JaegerGrpcSpanConverterTests()
        {
            _tracer = new Tracer.Builder("test-service-name")
                .WithReporter(new InMemoryReporter())
                .WithSampler(new ConstSampler(true))
                .Build();
        }

        public static List<object[]> DataProviderBuildTag() => new List<object[]>
        {
            new object[] { "value", GrpcTagType.String, "value" },
            new object[] { (long) 1, GrpcTagType.Int64, (long) 1 },
            new object[] { 1, GrpcTagType.Int64, (long) 1 },
            new object[] { (short) 1, GrpcTagType.Int64, (long) 1 },
            new object[] { (double) 1, GrpcTagType.Float64, (double) 1 },
            new object[] { (float) 1, GrpcTagType.Float64, (double) 1 },
            new object[] { (byte) 1, GrpcTagType.String, "1" },
            new object[] { true, GrpcTagType.Bool, true },
            // TODO C# doesn't use values for ToString() // new object[] { new List<string> { "hello" }, GrpcTagType.STRING, "[hello]" },
            new object[] { new List<string> { "hello" }, GrpcTagType.String, "System.Collections.Generic.List`1[System.String]" }
        };

        [Theory]
        [MemberData(nameof(DataProviderBuildTag))]
        public void TestBuildTag(object tagValue, GrpcTagType tagType, object expected)
        {
            GrpcTag tag = JaegerGrpcSpanConverter.BuildTag("key", tagValue);
            Assert.Equal(tagType, tag.VType);
            Assert.Equal("key", tag.Key);
            switch (tagType)
            {
                case GrpcTagType.Bool:
                    Assert.Equal(expected, tag.VBool);
                    break;
                case GrpcTagType.Int64:
                    Assert.Equal(expected, tag.VInt64);
                    break;
                case GrpcTagType.Float64:
                    Assert.Equal(expected, tag.VFloat64);
                    break;
                case GrpcTagType.Binary:
                    break;
                case GrpcTagType.String:
                default:
                    Assert.Equal(expected, tag.VStr);
                    break;
            }
        }

        [Fact]
        public void TestBuildTags()
        {
            var tags = new Dictionary<string, object> { { "key", "value" } };

            List<GrpcTag> grpcTags = JaegerGrpcSpanConverter.BuildTags(tags);
            Assert.NotNull(grpcTags);
            Assert.Single(grpcTags);
            Assert.Equal("key", grpcTags[0].Key);
            Assert.Equal("value", grpcTags[0].VStr);
            Assert.Equal(GrpcTagType.String, grpcTags[0].VType);
        }

        [Fact]
        public void TestConvertSpan()
        {
            var logTimestamp = new DateTimeOffset(2018, 4, 13, 10, 30, 0, TimeSpan.Zero);
            var fields = new Dictionary<string, object> { { "k", "v" } };

            Span span = (Span)_tracer.BuildSpan("operation-name").Start();
            span.Log(logTimestamp, fields);
            span.SetBaggageItem("foo", "bar");

            GrpcSpan grpcSpan = JaegerGrpcSpanConverter.ConvertSpan(span);

            Assert.Equal("operation-name", grpcSpan.OperationName);
            Assert.Equal(2, grpcSpan.Logs.Count);
            GrpcLog grpcLog = grpcSpan.Logs[0];
            Assert.Equal(logTimestamp, grpcLog.Timestamp.ToDateTimeOffset());
            Assert.Single(grpcLog.Fields);
            GrpcTag grpcTag = grpcLog.Fields[0];
            Assert.Equal("k", grpcTag.Key);
            Assert.Equal("v", grpcTag.VStr);

            // NOTE: In Java, the order is different (event, value, key) because the HashMap algorithm is different.
            grpcLog = grpcSpan.Logs[1];
            Assert.Equal(3, grpcLog.Fields.Count);
            grpcTag = grpcLog.Fields[0];
            Assert.Equal("event", grpcTag.Key);
            Assert.Equal("baggage", grpcTag.VStr);
            grpcTag = grpcLog.Fields[1];
            Assert.Equal("key", grpcTag.Key);
            Assert.Equal("foo", grpcTag.VStr);
            grpcTag = grpcLog.Fields[2];
            Assert.Equal("value", grpcTag.Key);
            Assert.Equal("bar", grpcTag.VStr);
        }

        [Fact]
        public void TestConvertSpanOneReferenceChildOf()
        {
            Span parent = (Span)_tracer.BuildSpan("foo").Start();

            Span child = (Span)_tracer.BuildSpan("foo")
                .AsChildOf(parent)
                .Start();

            GrpcSpan span = JaegerGrpcSpanConverter.ConvertSpan(child);

            // TODO: Check ParentSpanID
            //Assert.Equal((long)child.Context.ParentId, span.ParentSpanId);
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

            GrpcSpan span = JaegerGrpcSpanConverter.ConvertSpan(child);

            // TODO: Check ParentSpanID
            //Assert.Equal(0, span.ParentSpanId);
            Assert.Equal(2, span.References.Count);
            Assert.Equal(BuildReference(parent.Context, References.ChildOf), span.References[0]);
            Assert.Equal(BuildReference(parent2.Context, References.ChildOf), span.References[1]);
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

            GrpcSpan span = JaegerGrpcSpanConverter.ConvertSpan(child);

            // TODO: Check ParentSpanID
            //Assert.Equal(0, span.ParentSpanId);
            Assert.Equal(2, span.References.Count);
            Assert.Equal(BuildReference(parent.Context, References.FollowsFrom), span.References[0]);
            Assert.Equal(BuildReference(parent2.Context, References.ChildOf), span.References[1]);
        }

        private static GrpcReference BuildReference(SpanContext context, string referenceType)
        {
            return JaegerGrpcSpanConverter.BuildReferences(new List<Reference> { new Reference(context, referenceType) }.AsReadOnly())[0];
        }
    }
}