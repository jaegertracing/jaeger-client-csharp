using System.Text;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class TraceResponse
    {
        [JsonProperty("notImplementedError")]
        public string NotImplementedError { get; } = string.Empty;

        [JsonProperty("span")]
        public ObservedSpan Span { get; }

        [JsonProperty("downstream")]
        public TraceResponse Downstream { get; set; }

        public TraceResponse(ObservedSpan span)
        {
            Span = span;
        }

        [JsonConstructor]
        public TraceResponse(
            string notImplementedError,
            ObservedSpan span,
            TraceResponse downstream)
        {
            NotImplementedError = notImplementedError;
            Span = span;
            Downstream = downstream;
        }

        public TraceResponse(string notImplementedError)
        {
            NotImplementedError = notImplementedError;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("TraceResponse(");
            bool __first = true;
            if (Span != null)
            {
                __first = false;
                sb.Append("Span: ");
                sb.Append(Span == null ? "<null>" : Span.ToString());
            }
            if (Downstream != null)
            {
                if (!__first) { sb.Append(", "); }
                __first = false;
                sb.Append("Downstream: ");
                sb.Append(Downstream);
            }
            if (!__first) { sb.Append(", "); }
            sb.Append("NotImplementedError: ");
            sb.Append(NotImplementedError);
            sb.Append(")");
            return sb.ToString();
        }
    }
}