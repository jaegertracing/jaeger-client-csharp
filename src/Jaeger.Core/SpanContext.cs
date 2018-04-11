using System;
using System.Collections.Generic;
using OpenTracing;

namespace Jaeger.Core
{
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