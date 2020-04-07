using System.Text;
using System.Text.Json.Serialization;

namespace Jaeger.Crossdock.Model
{
    public class StartTraceRequest
    {
        [JsonPropertyName("sampled")]
        public bool Sampled { get; set; }

        [JsonPropertyName("baggage")]
        public string Baggage { get; set; }

        [JsonPropertyName("downstream")]
        public Downstream Downstream { get; set; }

        [JsonPropertyName("serverRole")]
        public string ServerRole { get; set; }

        public StartTraceRequest()
        {
        }

        public StartTraceRequest(
            string serverRole,
            bool sampled,
            string baggage,
            Downstream downstream)
        {
            ServerRole = serverRole;
            Sampled = sampled;
            Baggage = baggage;
            Downstream = downstream;
        }
        public override string ToString()
        {
            var sb = new StringBuilder("StartTraceRequest(");
            sb.Append(", ServerRole: ");
            sb.Append(ServerRole);
            sb.Append(", Sampled: ");
            sb.Append(Sampled);
            sb.Append(", Baggage: ");
            sb.Append(Baggage);
            sb.Append(", Downstream: ");
            sb.Append(Downstream == null ? "<null>" : Downstream.ToString());
            sb.Append(")");
            return sb.ToString();
        }
    }
}