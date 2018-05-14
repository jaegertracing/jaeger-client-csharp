using System;
using System.Globalization;
using Jaeger.Core.Util;

namespace Jaeger.Core
{
    /// <summary>
    /// Represents a unique 64bit identifier of a span.
    /// </summary>
    public readonly struct SpanId
    {
        private long Id { get; }

        public static SpanId NewUniqueId()
        {
            return new SpanId(Utils.UniqueId());
        }

        public SpanId(long spanId)
        {
            Id = spanId;
        }

        public SpanId(TraceId traceId)
        {
            Id = traceId.Low;
        }

        public override string ToString()
        {
            return Id.ToString("x");
        }

        public static implicit operator long(SpanId s)
        {
            return s.Id;
        }

        public static SpanId FromString(string from)
        {
            if (from.Length > 16)
            {
                throw new Exception($"SpanId cannot be longer than 16 hex characters: {from}");
            }

            if (!long.TryParse(from, NumberStyles.HexNumber, null, out var result))
            {
                throw new Exception($"Cannot parse SpanId from string: {from}");
            }

            return new SpanId(result);
        }
    }
}