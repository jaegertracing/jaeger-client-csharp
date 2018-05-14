using System;
using System.Collections.Generic;
using System.Text;
using OpenTracing.Propagation;

namespace Jaeger.Core.Propagation
{
    public class TextMapCodec : Codec<ITextMap>
    {
        /// <summary>
        /// Key used to store serialized span context representation.
        /// </summary>
        private const string SpanContextKey = "uber-trace-id";

        /// <summary>
        /// Key prefix used for baggage items.
        /// </summary>
        private const string BaggageKeyPrefix = "uberctx-";

        private static readonly PrefixedKeys Keys = new PrefixedKeys();

        private readonly string _contextKey;
        private readonly string _baggagePrefix;
        private readonly bool _urlEncoding;

        public TextMapCodec(bool urlEncoding)
            : this(new Builder().WithUrlEncoding(urlEncoding))
        {
        }

        private TextMapCodec(Builder builder)
        {
            _urlEncoding = builder.UrlEncoding;
            _contextKey = builder.SpanContextKey;
            _baggagePrefix = builder.BaggagePrefix;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer
                .Append("TextMapCodec{")
                .Append("contextKey=")
                .Append(_contextKey)
                .Append(',')
                .Append("baggagePrefix=")
                .Append(_baggagePrefix)
                .Append(',')
                .Append("urlEncoding=")
                .Append(_urlEncoding ? "true" : "false")
                .Append('}');
            return buffer.ToString();
        }

        protected override void Inject(SpanContext spanContext, ITextMap carrier)
        {
            carrier.Set(_contextKey, EncodedValue(spanContext.ContextAsString()));
            foreach (var entry in spanContext.GetBaggageItems())
            {
                carrier.Set(Keys.PrefixedKey(entry.Key, _baggagePrefix), EncodedValue(entry.Value));
            }
        }

        protected override SpanContext Extract(ITextMap carrier)
        {
            SpanContext context = null;
            Dictionary<string, string> baggage = null;
            string debugId = null;

            foreach (var entry in carrier)
            {
                // TODO there should be no lower-case here
                string key = entry.Key.ToLowerInvariant();
                if (string.Equals(key, _contextKey, StringComparison.Ordinal))
                {
                    context = SpanContext.ContextFromString(DecodedValue(entry.Value));
                }
                else if (string.Equals(key, Constants.DebugIdHeaderKey, StringComparison.Ordinal))
                {
                    debugId = DecodedValue(entry.Value);
                }
                else if (key.StartsWith(_baggagePrefix, StringComparison.Ordinal))
                {
                    if (baggage == null)
                    {
                        baggage = new Dictionary<string, string>();
                    }
                    baggage[Keys.UnprefixedKey(key, _baggagePrefix)] = DecodedValue(entry.Value);
                }
            }
            if (context == null)
            {
                if (debugId != null)
                {
                    return SpanContext.WithDebugId(debugId);
                }
                return null;
            }
            if (baggage == null)
            {
                return context;
            }
            return context.WithBaggage(baggage);
        }

        private string EncodedValue(string value)
        {
            if (!_urlEncoding)
            {
                return value;
            }
            try
            {
                return Uri.EscapeDataString(value);
            }
            catch (Exception)
            {
                // not much we can do, try raw value
                return value;
            }
        }

        private string DecodedValue(string value)
        {
            if (!_urlEncoding)
            {
                return value;
            }
            try
            {
                return Uri.UnescapeDataString(value);
            }
            catch (Exception)
            {
                // not much we can do, try raw value
                return value;
            }
        }

        public sealed class Builder
        {
            public bool UrlEncoding { get; private set; }
            public string SpanContextKey { get; private set; } = TextMapCodec.SpanContextKey;
            public string BaggagePrefix { get; private set; } = BaggageKeyPrefix;

            public Builder()
            {
            }

            public Builder WithUrlEncoding(bool urlEncoding)
            {
                UrlEncoding = urlEncoding;
                return this;
            }

            public Builder WithSpanContextKey(string spanContextKey)
            {
                SpanContextKey = spanContextKey;
                return this;
            }

            public Builder WithBaggagePrefix(string baggagePrefix)
            {
                BaggagePrefix = baggagePrefix;
                return this;
            }

            public TextMapCodec Build()
            {
                return new TextMapCodec(this);
            }
        }
    }
}