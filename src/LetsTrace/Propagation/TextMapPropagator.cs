using System;
using System.Collections.Generic;
using System.Web;
using OpenTracing;
using OpenTracing.Propagation;

namespace LetsTrace.Propagation
{
    public class TextMapPropagator : IInjector, IExtractor
    {
        private IHeadersConfig _headersConfig { get; }

        // functions to encode and decode strings so that they are safe over
        // the wire
        private Func<string, string> _encodeValue { get; }
        private Func<string, string> _decodeValue { get; }

        public TextMapPropagator(IHeadersConfig headersConfig, Func<string, string> encodeValue, Func<string, string> decodeValue)
        {
            _headersConfig = headersConfig ?? throw new ArgumentNullException(nameof(headersConfig));
            _encodeValue = encodeValue ?? throw new ArgumentNullException(nameof(encodeValue));
            _decodeValue = decodeValue ?? throw new ArgumentNullException(nameof(decodeValue));
        }

        public static TextMapPropagator NewTextMapPropagator(IHeadersConfig headersConfig)
        {
            return new TextMapPropagator(headersConfig, (val) => val, (val) => val);
        }

        public static TextMapPropagator NewHTTPHeaderPropagator(IHeadersConfig headersConfig)
        {
            return new TextMapPropagator(headersConfig, HttpUtility.UrlEncode, HttpUtility.UrlDecode);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, TCarrier carrier)
        {
            if (carrier is ITextMap map)
            {
                map.Set(_headersConfig.TraceContextHeaderName, _encodeValue(spanContext.ToString()));

                foreach(var baggage in spanContext.GetBaggageItems())
                {
                    map.Set($"{_headersConfig.TraceBaggageHeaderPrefix}-{baggage.Key}", _encodeValue(baggage.Value));
                }
                return;
            }
            throw new ArgumentException($"{nameof(carrier)} is not ITextMap");
        }

        public ISpanContext Extract<TCarrier>(TCarrier carrier)
        {
            if (carrier is ITextMap map)
            {
                var baggage = new Dictionary<string, string>();
                SpanContext context = null;

                foreach(var item in map)
                {
                    if (item.Key == _headersConfig.TraceContextHeaderName) {
                        var safeValue = _decodeValue(item.Value);
                        context = SpanContext.FromString(safeValue);
                    } else if (item.Key.StartsWith(_headersConfig.TraceBaggageHeaderPrefix)) {
                        var safeKey = RemoveBaggageKeyPrefix(item.Key);
                        var safeValue = _decodeValue(item.Value);
                        baggage.Add(safeKey, safeValue);
                    }
                }

                if (context == null) {
                    return null;
                }

                return context.SetBaggageItems(baggage);
            }
            throw new ArgumentException($"{nameof(carrier)} is not ITextMap");
        }

        private string RemoveBaggageKeyPrefix(string key)
        {
            return key.Replace($"{_headersConfig.TraceBaggageHeaderPrefix}-", "");
        }
    }
}