using System.Text;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class StartTraceRequest
    {
        [JsonProperty("sampled")]
        public bool Sampled { get; }

        [JsonProperty("baggage")]
        public string Baggage { get; }

        [JsonProperty("downstream")]
        public Downstream Downstream { get; }

        [JsonProperty("serverRole")]
        public string ServerRole { get; }

        [JsonConstructor]
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