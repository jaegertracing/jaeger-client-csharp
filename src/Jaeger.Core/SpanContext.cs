using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTracing;

namespace Jaeger.Core
{
    // TraceId represents unique 128bit identifier of a trace
    public class TraceId
    {
        public ulong High { get; }
        public ulong Low { get; }
        public bool IsValid => High != 0 || Low != 0;

        public TraceId(ulong low) : this(0, low)
        {
        }

        public TraceId(ulong high, ulong low)
        {
            High = high;
            Low = low;
        }

        public override string ToString()
        {
            if (High == 0)
            {
                return Low.ToString("x");
            }

            return $"{High:x}{Low:x016}";
        }

        public static TraceId FromString(string from)
        {
            if (from.Length > 32)
            {
                throw new Exception($"TraceId cannot be longer than 32 hex characters: {from}");
            }

            ulong high = 0, low = 0;

            if (from.Length > 16)
            {
                var highLength = from.Length - 16;
                var highString = from.Substring(0, highLength);
                var lowString = from.Substring(highLength);
                
                if (!ulong.TryParse(highString, NumberStyles.HexNumber, null, out high))
                {
                    throw new Exception($"Cannot parse High TraceId from string: {highString}");
                }

                if (!ulong.TryParse(lowString, NumberStyles.HexNumber, null, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {lowString}");
                }
            }
            else
            {
                if (!ulong.TryParse(from, NumberStyles.HexNumber, null, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {from}");
                }
            }

            return new TraceId(high, low);
        }
    }

    // SpanId represents unique 64bit identifier of a span
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

    // The SpanContext is used to pass the information needed by other spans so
    // that they can correctly reference other spans
    public class SpanContext : IJaegerCoreSpanContext
    {
        public TraceId TraceId { get; }
        public SpanId SpanId { get; }
        public SpanId ParentId { get; }
        public ContextFlags Flags { get; }

        private Dictionary<string, string> _baggage;

        public SpanContext(TraceId traceId, SpanId spanId = null, SpanId parentId = null, Dictionary<string, string> baggage = null, ContextFlags flags = ContextFlags.Sampled)
        {
            TraceId = traceId ?? throw new ArgumentNullException(nameof(traceId));
            ParentId = parentId;
            SpanId = spanId;
            Flags = flags;
            _baggage = baggage ?? new Dictionary<string, string>();
        }

        public bool IsSampled => Flags.HasFlag(ContextFlags.Sampled);

        public override string ToString() {
            return $"{TraceId}:{SpanId}:{ParentId}:{(byte)Flags}";
        }

        public static SpanContext FromString(string from) {
            if (from == string.Empty) { throw new Exception("Cannot convert empty string to SpanContext"); }

            var parts = from.Split(':');
            if (parts.Length != 4) { throw new Exception("String does not match tracer state format"); }

            return new SpanContext(TraceId.FromString(parts[0]), SpanId.FromString(parts[1]), SpanId.FromString(parts[2]), null, (ContextFlags)byte.Parse(parts[3]));
        }

        // OpenTracing API: Iterate through all baggage items
        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return _baggage;
        }

        internal ISpanContext SetBaggageItems(Dictionary<string, string> baggage)
        {
            _baggage = baggage;
            return this;
        }
    }
}