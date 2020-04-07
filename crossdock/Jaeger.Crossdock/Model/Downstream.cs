using System.Text;
using System.Text.Json.Serialization;

namespace Jaeger.Crossdock.Model
{
    public class Downstream
    {
        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public string Port { get; set; }

        [JsonPropertyName("transport")]
        public string Transport { get; set; }

        [JsonPropertyName("serverRole")]
        public string ServerRole { get; set; }

        [JsonPropertyName("downstream")]
        public Downstream Downstream_ { get; set; }

        public Downstream()
        {
        }

        public Downstream(
            string serviceName,
            string host,
            string port,
            string transport,
            string serverRole,
            Downstream downstream)
        {
            ServiceName = serviceName;
            Host = host;
            Port = port;
            Transport = transport;
            ServerRole = serverRole;
            Downstream_ = downstream;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Downstream(");
            sb.Append(", ServiceName: ");
            sb.Append(ServiceName);
            sb.Append(", ServerRole: ");
            sb.Append(ServerRole);
            sb.Append(", Host: ");
            sb.Append(Host);
            sb.Append(", Port: ");
            sb.Append(Port);
            sb.Append(", Transport: ");
            sb.Append(Transport);
            if (Downstream_ != null)
            {
                sb.Append(", Downstream_: ");
                sb.Append(Downstream_);
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}