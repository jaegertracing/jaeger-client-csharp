using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTracing.Propagation;

namespace Jaeger.Propagation
{
    public class W3CTextMapCodec : Codec<ITextMap>
    {
        public const string TraceParentName = "traceparent";
        public const string TraceStateName = "tracestate";
        public const string TraceStateTracingSystemName = "jaeger";
        private const int W3CSpecVersion = 0; 

        protected override SpanContext Extract(ITextMap carrier)
        {
            var traceStateValue = GetTraceStateValue(carrier);
            
            if (traceStateValue.ContainsKey(TraceStateTracingSystemName)) {
                return SpanContext.ContextFromString(traceStateValue[TraceStateTracingSystemName]);
            }

            return null;
        }

        protected override void Inject(SpanContext spanContext, ITextMap carrier)
        {
            var commonFormat = $"{ W3CSpecVersion.ToString("X2") }-{ HexCodec.ToLowerHex(spanContext.TraceId.High, spanContext.TraceId.Low) }-{ HexCodec.ToLowerHex(spanContext.SpanId) }-{ ((byte)spanContext.Flags).ToString("X2") }";
            carrier.Set(TraceParentName, commonFormat);
            
            var traceState = GetTraceStateValue(carrier);
            traceState[TraceStateTracingSystemName] =  spanContext.ToString();

            carrier.Set(TraceStateName, BuildTraceStateValue(traceState));
        }

        // BuildTraceStateValue takes a dictionary and returns the entries as <key>=<value> items seperated by a comma
        private static string BuildTraceStateValue(Dictionary<string, string> traceStateValueDict)
        {
            return string.Join(",", traceStateValueDict.Select(x => $"{ x.Key }={ x.Value }"));
        }

        // GetTraceStateValue extracts the trace state item from the text map passed in
        private static Dictionary<string, string> GetTraceStateValue(ITextMap textMap)
        {
            foreach (var item in textMap)
            {
                if (item.Key.ToLower() == TraceStateName) {
                    return item.Value.Split(',').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1],  StringComparer.OrdinalIgnoreCase);
                }
            }

            return new Dictionary<string, string>();
        }
    }
}
