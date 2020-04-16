using System.Text;
using System.Text.Json.Serialization;

namespace Jaeger.Crossdock.Model
{
    public class JoinTraceRequest
    {
        [JsonPropertyName("serverRole")]
        public string ServerRole { get; set; }

        [JsonPropertyName("downstream")]
        public Downstream Downstream { get; set; }

        public JoinTraceRequest()
        {
        }

        public JoinTraceRequest(string serverRole, Downstream downstream)
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