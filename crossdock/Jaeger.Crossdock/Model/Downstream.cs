﻿using System.Text;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class Downstream
    {
        [JsonProperty("serviceName")]
        public string ServiceName { get; }

        [JsonProperty("host")]
        public string Host { get; }

        [JsonProperty("port")]
        public string Port { get; }

        [JsonProperty("transport")]
        public string Transport { get; }

        [JsonProperty("serverRole")]
        public string ServerRole { get; }

        [JsonProperty("downstream")]
        public Downstream Downstream_ { get; }

        [JsonConstructor]
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