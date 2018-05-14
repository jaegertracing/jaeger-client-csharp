using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using OpenTracing;

namespace Jaeger.Core
{
    public class SpanContext : ISpanContext
    {
        internal static readonly IReadOnlyDictionary<string, string> EmptyBaggage = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        public TraceId TraceId { get; }
        public SpanId SpanId { get; }
        public SpanId ParentId { get; }
        public SpanContextFlags Flags { get; }
        internal IReadOnlyDictionary<string, string> Baggage { get; }
        internal string DebugId { get; }

        public bool IsSampled => Flags.HasFlag(SpanContextFlags.Sampled);

        public bool IsDebug => Flags.HasFlag(SpanContextFlags.Debug);

        public SpanContext(TraceId traceId, SpanId spanId, SpanId parentId, SpanContextFlags flags)
            : this(traceId, spanId, parentId, flags, EmptyBaggage, debugId: null)
        {
        }

        internal SpanContext(
            TraceId traceId,
            SpanId spanId,
            SpanId parentId,
            SpanContextFlags flags,
            IReadOnlyDictionary<string, string> baggage,
            string debugId)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentId = parentId;
            Flags = flags;
            Baggage = baggage ?? throw new ArgumentNullException(nameof(baggage));
            DebugId = debugId;
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return Baggage;
        }

        public string GetBaggageItem(string key)
        {
            return Baggage.TryGetValue(key, out string value) ? value : null;
        }

        public string ContextAsString()
        {
            return $"{TraceId}:{SpanId}:{ParentId}:{((byte)Flags).ToString("x")}";
        }

        public override string ToString()
        {
            return ContextAsString();
        }

        public static SpanContext ContextFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Cannot convert empty string to {nameof(SpanContext)}");

            var parts = value.Split(':');
            if (parts.Length != 4)
                throw new ArgumentException("String does not match tracer state format");

            return new SpanContext(
                TraceId.FromString(parts[0]),
                SpanId.FromString(parts[1]),
                SpanId.FromString(parts[2]),
                (SpanContextFlags)byte.Parse(parts[3], NumberStyles.HexNumber));
        }

        public SpanContext WithBaggageItem(string key, string value)
        {
            var newBaggage = new Dictionary<string, string>();
            foreach (var oldBaggageItem in Baggage)
            {
                newBaggage[oldBaggageItem.Key] = oldBaggageItem.Value;
            }

            if (value == null)
            {
                newBaggage.Remove(key);
            }
            else
            {
                newBaggage[key] = value;
            }
            return new SpanContext(TraceId, SpanId, ParentId, Flags, newBaggage, DebugId);
        }

        public SpanContext WithBaggage(Dictionary<string, string> newBaggage)
        {
            return new SpanContext(TraceId, SpanId, ParentId, Flags, newBaggage, DebugId);
        }

        public SpanContext WithFlags(SpanContextFlags flags)
        {
            return new SpanContext(TraceId, SpanId, ParentId, flags, Baggage, DebugId);
        }

        /// <summary>
        /// Returns <c>true</c> when the instance of the context is only used to return the debug/correlation ID
        /// from <see cref="ITracer.Extract"/> method. This happens in the situation when "jaeger-debug-id" header is passed in
        /// the carrier to the extract method, but the request otherwise has no span context in it.
        /// Previously this would've returned <c>null</c> from the extract method, but now it returns a dummy
        /// context with only debugId filled in.
        /// </summary>
        /// <seealso cref="Constants.DebugIdHeaderKey"/>
        internal bool IsDebugIdContainerOnly()
        {
            return TraceId.IsZero && DebugId != null;
        }

        /// <summary>
        /// Create a new dummy <see cref="SpanContext"/> as a container for <paramref name="debugId"/> string.
        /// This is used when "jaeger-debug-id" header is passed in the request headers and forces the trace to be sampled as
        /// debug trace, and the value of header recorded as a span tag to serve as a searchable
        /// correlation ID.
        /// </summary>
        /// <param name="debugId">Arbitrary string used as correlation ID</param>
        /// <returns>New dummy <see cref="SpanContext"/> that serves as a container for debugId only.</returns>
        /// <seealso cref="Constants.DebugIdHeaderKey"/>
        public static SpanContext WithDebugId(string debugId)
        {
            return new SpanContext(new TraceId(0), new SpanId(0), new SpanId(0), SpanContextFlags.None, EmptyBaggage, debugId);
        }
    }
}