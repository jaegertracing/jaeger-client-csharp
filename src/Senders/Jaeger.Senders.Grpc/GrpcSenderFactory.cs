using System.IO;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Jaeger.Senders.Grpc
{
    public class GrpcSenderFactory : ISenderFactory
    {
        public const string Name = "grpc";

        public ISender GetSender(ILoggerFactory loggerFactory, Configuration.SenderConfiguration senderConfiguration)
        {
            var logger = loggerFactory.CreateLogger<GrpcSenderFactory>();

            ChannelCredentials credentials;
            if (!string.IsNullOrEmpty(senderConfiguration.GrpcRootCertificate))
            {
                logger.LogDebug("Using TLS gRPC channel with data from the configuration.");
                
                KeyCertificatePair keypair = null;
                if (!string.IsNullOrEmpty(senderConfiguration.GrpcClientChain)
                    && !string.IsNullOrEmpty(senderConfiguration.GrpcClientKey))
                {
                    var clientcert = File.ReadAllText(senderConfiguration.GrpcClientChain);
                    var clientkey = File.ReadAllText(senderConfiguration.GrpcClientKey);
                    keypair = new KeyCertificatePair(clientcert, clientkey);
                }

                var rootcert = File.ReadAllText(senderConfiguration.GrpcRootCertificate);
                credentials = new SslCredentials(rootcert, keypair);
            }
            else
            {
                logger.LogDebug("Using insecure gRPC channel without credentials.");
                credentials = ChannelCredentials.Insecure;
            }

            logger.LogDebug("Using the gRPC Sender to send spans directly to the endpoint.");
            return new GrpcSender(
                StringOrDefault(senderConfiguration.GrpcTarget, GrpcSender.DefaultCollectorGrpcTarget),
                credentials,
                0 /* max packet size */);
        }

        public string FactoryName => Name;

        private static string StringOrDefault(string value, string defaultValue)
        {
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        public override string ToString()
        {
            return $"{nameof(GrpcSenderFactory)}(FactoryName = {FactoryName})";
        }
    }
}
