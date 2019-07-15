using System.Text;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class ObservedSpan
    {
        [JsonProperty("traceId")]
        public string TraceId { get; }

        [JsonProperty("sampled")]
        public bool Sampled { get; }

        [JsonProperty("baggage")]
        public string Baggage { get; }

        [JsonConstructor]
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