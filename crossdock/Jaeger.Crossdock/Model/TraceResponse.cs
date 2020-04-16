using System.Text;
using System.Text.Json.Serialization;

namespace Jaeger.Crossdock.Model
{
    public class TraceResponse
    {
        [JsonPropertyName("notImplementedError")]
        public string NotImplementedError { get; set; } = string.Empty;

        [JsonPropertyName("span")]
        public ObservedSpan Span { get; set; }

        [JsonPropertyName("downstream")]
        public TraceResponse Downstream { get; set; }

        public TraceResponse()
        {
        }

        public TraceResponse(ObservedSpan span)
        {
            Span = span;
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