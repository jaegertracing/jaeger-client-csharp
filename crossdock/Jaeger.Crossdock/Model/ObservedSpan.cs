using System.Text;
using System.Text.Json.Serialization;

namespace Jaeger.Crossdock.Model
{
    public class ObservedSpan
    {
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("sampled")]
        public bool Sampled { get; set; }

        [JsonPropertyName("baggage")]
        public string Baggage { get; set; }

        public ObservedSpan()
        {
        }

        public ObservedSpan(
            string traceId,
            bool sampled,
            string baggage)
        {
            TraceId = traceId;
            Sampled = sampled;
            Baggage = baggage;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ObservedSpan(");
            sb.Append(", TraceId: ");
            sb.Append(TraceId);
            sb.Append(", Sampled: ");
            sb.Append(Sampled);
            sb.Append(", Baggage: ");
            sb.Append(Baggage);
            sb.Append(")");
            return sb.ToString();
        }
    }
}