using System;
using System.Collections.Generic;
using System.Linq;
using LetsTrace.Propagation;
using NSubstitute;
using OpenTracing;
using Xunit;

namespace LetsTrace.Tests
{
    public class PropagationTests
    {
        [Fact]
        public void TextMapPropagator_Constructor_ShouldThrowIfHeadersConfigIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TextMapPropagator(null, null, null));
            Assert.Equal("headersConfig", ex.ParamName);
        }

        [Fact]
        public void TextMapPropagator_Constructor_ShouldThrowIfEncodeIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TextMapPropagator(new HeadersConfig("", ""), null, null));
            Assert.Equal("encodeValue", ex.ParamName);
        }

        [Fact]
        public void TextMapPropagator_Constructor_ShouldThrowIfDecodeIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TextMapPropagator(new HeadersConfig("", ""), (val) => val, null));
            Assert.Equal("decodeValue", ex.ParamName);
        }

        [Fact]
        public void TextMapPropagator_Inject_ThrowsWhenCarrierIsNotITextMap()
        {
            var propagator = new TextMapPropagator(new HeadersConfig("", ""), (val) => val, (val) => val);
            var spanContext = Substitute.For<ISpanContext>();

            var ex = Assert.Throws<ArgumentException>(() => propagator.Inject(spanContext, new List<string>()));
            Assert.Equal("carrier is not ITextMap", ex.Message);
        }

        [Fact]
        public void TextMapPropagator_Inject_SetsTheRightHeadersWithTheRightData()
        {
            var headersConfig = new HeadersConfig("TraceContextHeaderName", "TraceBaggageHeaderPrefix");
            var propagator = new TextMapPropagator(headersConfig, (val) => val, (val) => val);
            var baggage = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var spanContext = Substitute.For<ISpanContext>();
            var carrier = new DictionaryTextMap();

            spanContext.GetBaggageItems().Returns(baggage);

            propagator.Inject(spanContext, carrier);

            var carrierDict = carrier.ToDictionary(c => c.Key, c => c.Value);

            Assert.Equal("Castle.Proxies.ISpanContextProxy", carrierDict[headersConfig.TraceContextHeaderName]); // cannot mock ToString
            Assert.Equal(baggage["key1"], carrierDict[$"{headersConfig.TraceBaggageHeaderPrefix}-key1"]);
            Assert.Equal(baggage["key2"], carrierDict[$"{headersConfig.TraceBaggageHeaderPrefix}-key2"]);
        }

        [Fact]
        public void TextMapPropagator_Extract_ThrowsWhenCarrierIsNotITextMap()
        {
            var propagator = new TextMapPropagator(new HeadersConfig("", ""), (val) => val, (val) => val);
            var spanContext = Substitute.For<ISpanContext>();

            var ex = Assert.Throws<ArgumentException>(() => propagator.Extract(new List<string>()));
            Assert.Equal("carrier is not ITextMap", ex.Message);
        }

        [Fact]
        public void TextMapPropagator_Extract_SetsTheRightHeadersWithTheRightData()
        {
            var headersConfig = new HeadersConfig("TraceContextHeaderName", "TraceBaggageHeaderPrefix");
            var propagator = new TextMapPropagator(headersConfig, (val) => val, (val) => val);
            var carrier = new DictionaryTextMap(new Dictionary<string, string> { 
                { "TraceContextHeaderName", "1:2:3:4" },
                { "TraceBaggageHeaderPrefix-Item1", "item1" },
                { "TraceBaggageHeaderPrefix-Item2", "item2" },
            });

            var sc = (SpanContext) propagator.Extract(carrier);

            var baggage = sc.GetBaggageItems().ToDictionary(kv => kv.Key, kv => kv.Value);
            Assert.Equal("item1", baggage["Item1"]);
            Assert.Equal("item2", baggage["Item2"]);
            Assert.Equal("1", sc.TraceId.Low.ToString());
            Assert.Equal("2", sc.SpanId.ToString());
            Assert.Equal("3", sc.ParentId.ToString());
            Assert.Equal(4, (byte)sc.Flags);
        }
    }
}
