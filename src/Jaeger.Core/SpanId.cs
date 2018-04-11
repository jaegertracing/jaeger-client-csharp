using System;
using System.Globalization;

namespace Jaeger.Core
{
    /// <summary>
    /// Represents a unique 64bit identifier of a span.
    /// </summary>
    public class SpanId
    {
        private ulong Id { get; }

        public SpanId(ulong spanId)
        {
            Id = spanId;
        }

        public static implicit operator ulong(SpanId s)
        {
            return s.Id;
        }

        public static implicit operator long(SpanId s)
        {
            return (long)s.Id;
        }

        public override string ToString()
        {
            return Id.ToString("x");
        }

        public static SpanId FromString(string from) {
            if (from.Length > 16)
            {
                throw new Exception($"SpanId cannot be longer than 16 hex characters: {from}");
            }

            if (!ulong.TryParse(from, NumberStyles.HexNumber, null, out var result))
            {
                throw new Exception($"Cannot parse SpanId from string: {from}");
            }

            return new SpanId(result);
        }
    }
}