using Microsoft.Extensions.Logging;

namespace Jaeger.Senders.Thrift
{
    public class ThriftSenderFactory : ISenderFactory
    {
        public ISender GetSender(ILoggerFactory loggerFactory, Configuration.SenderConfiguration senderConfiguration)
        {
            var logger = loggerFactory.CreateLogger<ThriftSenderFactory>();

            if (!string.IsNullOrEmpty(senderConfiguration.Endpoint))
            {
                HttpSender.Builder httpSenderBuilder = new HttpSender.Builder(senderConfiguration.Endpoint);
                if (!string.IsNullOrEmpty(senderConfiguration.AuthUsername) && !string.IsNullOrEmpty(senderConfiguration.AuthPassword))
                {
                    logger.LogDebug("Using HTTP Basic authentication with data from the environment variables.");
                    httpSenderBuilder.WithAuth(senderConfiguration.AuthUsername, senderConfiguration.AuthPassword);
                }
                else if (!string.IsNullOrEmpty(senderConfiguration.AuthToken))
                {
                    logger.LogDebug("Auth Token environment variable found.");
                    httpSenderBuilder.WithAuth(senderConfiguration.AuthToken);
                }

                logger.LogDebug("Using the HTTP Sender to send spans directly to the endpoint.");
                return httpSenderBuilder.Build();
            }

            logger.LogDebug("Using the UDP Sender to send spans to the agent.");
            return new UdpSender(
                    StringOrDefault(senderConfiguration.AgentHost, UdpSender.DefaultAgentUdpHost),
                    senderConfiguration.AgentPort.GetValueOrDefault(UdpSender.DefaultAgentUdpCompactPort),
                    0 /* max packet size */);
        }

        public string FactoryName => "thrift";

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
