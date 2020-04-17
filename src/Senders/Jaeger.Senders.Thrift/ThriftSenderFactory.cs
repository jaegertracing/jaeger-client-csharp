using Jaeger.Transports.Thrift;
using Microsoft.Extensions.Logging;

namespace Jaeger.Senders.Thrift
{
    public class ThriftSenderFactory : ISenderFactory
    {
        public const string Name = "thrift";

        public ISender GetSender(ILoggerFactory loggerFactory, Configuration.SenderConfiguration senderConfiguration)
        {
            var logger = loggerFactory.CreateLogger<ThriftSenderFactory>();

            if (!string.IsNullOrEmpty(senderConfiguration.Endpoint))
            {
                var httpBuilder = new ThriftHttpTransport.Builder(senderConfiguration.Endpoint);
                if (!string.IsNullOrEmpty(senderConfiguration.AuthUsername) && !string.IsNullOrEmpty(senderConfiguration.AuthPassword))
                {
                    logger.LogDebug("Using HTTP Basic authentication with data from the environment variables.");
                    httpBuilder.WithAuth(senderConfiguration.AuthUsername, senderConfiguration.AuthPassword);
                }
                else if (!string.IsNullOrEmpty(senderConfiguration.AuthToken))
                {
                    logger.LogDebug("Auth Token environment variable found.");
                    httpBuilder.WithAuth(senderConfiguration.AuthToken);
                }

                logger.LogDebug("Using the HTTP Sender to send spans directly to the endpoint.");
                return new HttpSender(
                    httpBuilder.Build(),
                    0);
            }

            logger.LogDebug("Using the UDP Sender to send spans to the agent.");
            return new UdpSender(
                new ThriftUdpTransport.Builder()
                    .WithHost(StringOrDefault(senderConfiguration.AgentHost, null))
                    .WithPort(senderConfiguration.AgentPort.GetValueOrDefault(0))
                    .Build(),
                0 /* max packet size */);
        }

        public string FactoryName => Name;

        private static string StringOrDefault(string value, string defaultValue)
        {
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        public override string ToString()
        {
            return $"{nameof(ThriftSenderFactory)}(FactoryName = {FactoryName})";
        }
    }
}
