using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LetsTrace.Propagation;
using NSubstitute;
using OpenTracing.Propagation;
using Xunit;

namespace LetsTrace.Tests.Propagation
{
    public class TextMapPropagationRegistryTests
    {
        private readonly IPropagationRegistry _propagationRegistry;
        private readonly Dictionary<string, string> _baggage;
        private readonly ILetsTraceSpanContext _spanContext;
        private readonly IFormat<ITextMap> _format;

        public TextMapPropagationRegistryTests()
        {
            _propagationRegistry = Propagators.TextMap;
            _baggage = new Dictionary<string, string> { { "key1", "value1/val" }, { "key2", "value2/val" } };
            _spanContext = Substitute.For<ILetsTraceSpanContext>();
            _format = Substitute.For<IFormat<ITextMap>>();

            _spanContext.GetBaggageItems().Returns(_baggage);
        }

        [Fact]
        public void TextMapPropagationRegistry_Constructor_ShouldThrowIfHeadersConfigIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TextMapPropagationRegistry(null));
            Assert.Equal("headersConfig", ex.ParamName);
        }

        [Fact]
        public void TextMapPropagationRegistry_Inject_ThrowsWhenFormatIsNotSupported()
        {
            var carrier = Substitute.For<ITextMap>();

            var ex = Assert.Throws<ArgumentException>(() => _propagationRegistry.Inject(_spanContext, _format, carrier));
            Assert.Equal($"{_format.GetType().FullName} is not a supported injection format\r\nParameter name: format", ex.Message);
        }

        [Fact]
        public void TextMapPropagationRegistry_Inject_TextMap_SetsTheRightHeadersWithTheRightData()
        {
            var carrier = new DictionaryTextMap();

            _propagationRegistry.Inject(_spanContext, BuiltinFormats.TextMap, carrier);

            var carrierDict = carrier.ToDictionary(c => c.Key, c => c.Value);

            Assert.Equal(_spanContext.GetType().FullName, carrierDict[Constants.TRACE_CONTEXT_HEADER_NAME]); // cannot mock ToString
            Assert.Equal(_baggage["key1"], carrierDict[$"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-key1"]);
            Assert.Equal(_baggage["key2"], carrierDict[$"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-key2"]);
        }

        [Fact]
        public void TextMapPropagationRegistry_Inject_HttpHeaders_SetsTheRightHeadersWithTheRightData()
        {
            var carrier = new DictionaryTextMap();

            _propagationRegistry.Inject(_spanContext, BuiltinFormats.HttpHeaders, carrier);

            var carrierDict = carrier.ToDictionary(c => c.Key, c => c.Value);

            Assert.Equal(_spanContext.GetType().FullName, carrierDict[Constants.TRACE_CONTEXT_HEADER_NAME]); // cannot mock ToString
            Assert.Equal(HttpUtility.UrlEncode(_baggage["key1"]), carrierDict[$"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-key1"]);
            Assert.Equal(HttpUtility.UrlEncode(_baggage["key2"]), carrierDict[$"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-key2"]);
        }

        [Fact]
        public void TextMapPropagationRegistry_Extract_ThrowsWhenFormatIsNotSupported()
        {
            var carrier = Substitute.For<ITextMap>();

            var ex = Assert.Throws<ArgumentException>(() => _propagationRegistry.Extract(_format, carrier));
            Assert.Equal($"{_format.GetType().FullName} is not a supported extraction format\r\nParameter name: format", ex.Message);
        }

        [Fact]
        public void TextMapPropagationRegistry_Extract_TraceContextHeaderMissing()
        {
            var carrier = new DictionaryTextMap(new Dictionary<string, string> {
                { $"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-Item1", "item1/val" },
                { $"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-Item2", "item2/val" },
            });

            var sc = (SpanContext)_propagationRegistry.Extract(BuiltinFormats.TextMap, carrier);
            Assert.Null(sc);

            sc = (SpanContext)_propagationRegistry.Extract(BuiltinFormats.HttpHeaders, carrier);
            Assert.Null(sc);
        }

        [Fact]
        public void TextMapPropagator_Extract_TextMap_SetsTheRightHeadersWithTheRightData()
        {
            var carrier = new DictionaryTextMap(new Dictionary<string, string> {
                { Constants.TRACE_CONTEXT_HEADER_NAME, "1:2:3:4" },
                { $"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-Item1", "item1/val" },
                { $"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-Item2", "item2/val" },
            });

            var sc = (SpanContext)_propagationRegistry.Extract(BuiltinFormats.TextMap, carrier);

            var baggage = sc.GetBaggageItems().ToDictionary(kv => kv.Key, kv => kv.Value);
            Assert.Equal("item1/val", baggage["Item1"]);
            Assert.Equal("item2/val", baggage["Item2"]);
            Assert.Equal("1", sc.TraceId.Low.ToString());
            Assert.Equal("2", sc.SpanId.ToString());
            Assert.Equal("3", sc.ParentId.ToString());
            Assert.Equal(4, (byte)sc.Flags);
        }

        [Fact]
        public void TextMapPropagator_Extract_HttpHeaders_SetsTheRightHeadersWithTheRightData()
        {
            var carrier = new DictionaryTextMap(new Dictionary<string, string> {
                { Constants.TRACE_CONTEXT_HEADER_NAME, "1:2:3:4" },
                { $"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-Item1", "item1%2fval" },
                { $"{Constants.TRACE_BAGGAGE_HEADER_PREFIX}-Item2", "item2%2fval" },
            });

            var sc = (SpanContext)_propagationRegistry.Extract(BuiltinFormats.HttpHeaders, carrier);

            var baggage = sc.GetBaggageItems().ToDictionary(kv => kv.Key, kv => kv.Value);
            Assert.Equal("item1/val", baggage["Item1"]);
            Assert.Equal("item2/val", baggage["Item2"]);
            Assert.Equal("1", sc.TraceId.Low.ToString());
            Assert.Equal("2", sc.SpanId.ToString());
            Assert.Equal("3", sc.ParentId.ToString());
            Assert.Equal(4, (byte)sc.Flags);
        }
    }
}
