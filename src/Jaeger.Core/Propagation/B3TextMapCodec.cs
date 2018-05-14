using System;
using OpenTracing.Propagation;

namespace Jaeger.Core.Propagation
{
    /// <summary>
    /// This format is compatible with other Zipkin based trace libraries such as Brave, Wingtips, zipkin-js, etc.
    /// <para/>
    /// Example usage:
    /// <code>
    /// var b3TextMapCodec = new B3TextMapCodec();
    /// tracer = new Tracer.Builder(serviceName)
    ///                    .With...()
    ///                    .RegisterCodec(BuiltinFormats.HttpHeaders, b3TextMapCodec)
    ///                    ...
    /// </code>
    /// <para/>
    /// See <a href="http://zipkin.io/pages/instrumenting.html">Instrumenting a Library</a>
    /// </summary>
    public class B3TextMapCodec : Codec<ITextMap>
    {
        public const string TraceIdName = "X-B3-TraceId";
        public const string SpanIdName = "X-B3-SpanId";
        public const string ParentSpanIdName = "X-B3-ParentSpanId";
        public const string SampledName = "X-B3-Sampled";
        public const string FlagsName = "X-B3-Flags";

        protected override void Inject(SpanContext spanContext, ITextMap carrier)
        {
            carrier.Set(TraceIdName, HexCodec.ToLowerHex(spanContext.TraceId.High, spanContext.TraceId.Low));
            if (spanContext.ParentId != 0L)
            {
                // Conventionally, parent id == 0 means the root span
                carrier.Set(ParentSpanIdName, HexCodec.ToLowerHex(spanContext.ParentId));
            }
            carrier.Set(SpanIdName, HexCodec.ToLowerHex(spanContext.SpanId));
            carrier.Set(SampledName, spanContext.IsSampled ? "1" : "0");
            if (spanContext.IsDebug)
            {
                carrier.Set(FlagsName, "1");
            }
        }

        protected override SpanContext Extract(ITextMap carrier)
        {
            long? traceId = null;
            long? spanId = null;
            long? parentId = 0L; // Conventionally, parent id == 0 means the root span
            SpanContextFlags flags = SpanContextFlags.None;

            foreach (var entry in carrier)
            {
                if (string.Equals(entry.Key, SampledName, StringComparison.OrdinalIgnoreCase))
                {
                    string value = entry.Value;
                    if (string.Equals(value, "1", StringComparison.Ordinal) || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        flags |= SpanContextFlags.Sampled;
                    }
                }
                else if (string.Equals(entry.Key, TraceIdName, StringComparison.OrdinalIgnoreCase))
                {
                    traceId = HexCodec.LowerHexToUnsignedLong(entry.Value);
                }
                else if (string.Equals(entry.Key, ParentSpanIdName, StringComparison.OrdinalIgnoreCase))
                {
                    parentId = HexCodec.LowerHexToUnsignedLong(entry.Value);
                }
                else if (string.Equals(entry.Key, SpanIdName, StringComparison.OrdinalIgnoreCase))
                {
                    spanId = HexCodec.LowerHexToUnsignedLong(entry.Value);
                }
                else if (string.Equals(entry.Key, FlagsName, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(entry.Value, "1", StringComparison.OrdinalIgnoreCase))
                    {
                        flags |= SpanContextFlags.Debug;
                    }
                }
            }

            if (traceId != null && parentId != null && spanId != null)
            {
                return new SpanContext(new TraceId(traceId.Value), new SpanId(spanId.Value), new SpanId(parentId.Value), flags);
            }
            return null;
        }
    }
}