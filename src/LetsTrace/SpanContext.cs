using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LetsTrace
{
    // TraceId represents unique 128bit identifier of a trace
    public class TraceId
    {
        public UInt64 High { get; set; }
        public UInt64 Low { get; set;}

        public override string ToString()
        {
            if (High == 0)
            {
                return Low.ToString("x");
            }

            return $"{High.ToString("x")}{Low.ToString("x016")}";
        }

        public bool IsValid()
        {
            return High != 0 || Low != 0;
        }

        public static TraceId FromString(string from)
        {
            if (from.Length > 32)
            {
                throw new Exception($"TraceId cannot be longer than 32 hex characters: {from}");
            }

            UInt64 high = 0, low = 0;

            if (from.Length > 15)
            {
                var highLength = from.Length - 16;
                var highString = from.Substring(0, highLength);
                var lowString = from.Substring(highLength);
                
                if (!UInt64.TryParse(highString, out high))
                {
                    throw new Exception($"Cannot parse High TraceId from string: {highString}");
                }

                if (!UInt64.TryParse(lowString, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {lowString}");
                }
            }
            else
            {
                if (!UInt64.TryParse(from, out low))
                {
                    throw new Exception($"Cannot parse Low TraceId from string: {from}");
                }
            }

            return new TraceId{ High = high, Low = low };
        }
    }

    // SpanId represents unique 64bit identifier of a span
    public class SpanId
    {
        private UInt64 _spanId;

        public SpanId(UInt64 spanId)
        {
            _spanId = spanId;
        }

        public static implicit operator UInt64(SpanId s)
        {
            return s._spanId;
        }

        public static implicit operator long(SpanId s)
        {
            return (long)s._spanId;
        }

        public override string ToString()
        {
            return _spanId.ToString("x");
        }

        public static SpanId FromString(string from) {
            if (from.Length > 16)
            {
                throw new Exception($"SpanId cannot be longer than 16 hex characters: {from}");
            }
            UInt64 result;
            
            if (!UInt64.TryParse(from, out result))
            {
                throw new Exception($"Cannot parse SpanId from string: {from}");
            }

            return new SpanId(result);
        }
    }

    // The SpanContext is used to pass the information needed by other spans so
    // that they can correctly reference other spans
    public class SpanContext : ILetsTraceSpanContext
    {
        public TraceId TraceId { get; }
        public SpanId SpanId { get; }
        public SpanId ParentId { get; }
        private Dictionary<string, string> _baggage;

        public SpanContext(TraceId traceId, SpanId spanId = null, SpanId parentId = null, Dictionary<string, string> baggage = null)
        {
            TraceId = traceId ?? throw new ArgumentNullException(nameof(traceId));
            ParentId = parentId;
            SpanId = spanId;
            _baggage = baggage ?? new Dictionary<string, string>();
        }

        public override string ToString() {
            return $"{TraceId.ToString()}:{SpanId.ToString()}:{ParentId.ToString()}";
        }

        public static SpanContext FromString(string from) {
            if (from == string.Empty) { throw new Exception("Cannot convert empty string to SpanContext"); }

            var parts = from.Split(':');
            if (parts.Length != 3) { throw new Exception("String does not match tracer state format"); }

            return new SpanContext(TraceId.FromString(parts[0]), SpanId.FromString(parts[1]), SpanId.FromString(parts[2]));
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