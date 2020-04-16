﻿using Grpc.Core;
using Jaeger.Encoders.Grpc;
using Jaeger.Transports.Grpc;
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
                credentials = new SslCredentials(senderConfiguration.GrpcRootCertificate);
            }
            else
            {
                logger.LogDebug("Using insecure gRPC channel without credentials.");
                credentials = ChannelCredentials.Insecure;
            }

            var transport = new GrpcTransport(
                StringOrDefault(senderConfiguration.GrpcTarget, GrpcTransport.DefaultCollectorGrpcTarget),
                credentials);

            logger.LogDebug("Using the gRPC Sender to send spans directly to the endpoint.");
            return new GrpcSender(
                new GrpcEncoder(transport),
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
