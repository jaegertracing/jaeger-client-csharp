using System;
using System.Globalization;
using Jaeger.Util;

namespace Jaeger
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

        public bool Equals(SpanId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is SpanId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString("x016");
        }

        public byte[] ToByteArray()
        {
            return Utils.LongToNetworkBytes(Id);
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