using Jaeger.Senders;
using Microsoft.Extensions.Logging;

namespace Jaeger.Core.Tests.Senders
{
    internal class FlexibleSenderFactory : ISenderFactory
    {
        internal class Sender : NoopSender
        {
            public string FactoryName { get; }
            public Configuration.SenderConfiguration SenderConfiguration { get; }

            public Sender(string factoryName, Configuration.SenderConfiguration senderConfiguration)
            {
                FactoryName = factoryName;
                SenderConfiguration = senderConfiguration;
            }
        }

        public FlexibleSenderFactory(string factoryName)
        {
            FactoryName = factoryName;
        }

        public ISender GetSender(ILoggerFactory loggerFactory, Configuration.SenderConfiguration senderConfiguration)
        {
            return new Sender(FactoryName, senderConfiguration);
        }

        public string FactoryName { get; }

        public override string ToString()
        {
            return $"{nameof(FlexibleSenderFactory)}(FactoryName = {FactoryName})";
        }
    }
}
