using System.Text;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class JoinTraceRequest
    {
        [JsonProperty("serverRole")]
        public string ServerRole { get; }

        [JsonProperty("downstream")]
        public Downstream Downstream { get; }

        [JsonConstructor]
        public JoinTraceRequest(
            string serverRole,
            Downstream downstream)
        {
            ServerRole = serverRole;
            Downstream = downstream;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("JoinTraceRequest(");
            sb.Append(", ServerRole: ");
            sb.Append(ServerRole);
            if (Downstream != null)
            {
                sb.Append(", Downstream: ");
                sb.Append(Downstream == null ? "<null>" : Downstream.ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}