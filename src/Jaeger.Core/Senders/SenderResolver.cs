using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Jaeger.Senders
{
    /// <summary>
    /// Provides a way to resolve an appropriate <see cref="ISender"/>.
    /// </summary>
    public class SenderResolver
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IDictionary<string, ISenderFactory> _senderFactories;

        public SenderResolver(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SenderResolver>();
            _senderFactories = new Dictionary<string, ISenderFactory>();
        }

        /// <summary>
        /// Register a <see cref="ISenderFactory"/> to the this SenderResolver. These will be checked 
        /// </summary>
        /// <typeparam name="T">The factory to register by it's <see cref="ISenderFactory.FactoryName"/></typeparam>
        /// <returns>A reference to itself for easy chaining</returns>
        public SenderResolver RegisterSenderFactory<T>() where T : ISenderFactory, new()
        {
            ISenderFactory factory = new T();
            return RegisterSenderFactory(factory);
        }

        /// <summary>
        /// Register a <see cref="ISenderFactory"/> to the this SenderResolver. These will be checked 
        /// </summary>
        /// <param name="factory">The factory to register by it's <see cref="ISenderFactory.FactoryName"/></param>
        /// <returns>A reference to itself for easy chaining</returns>
        public SenderResolver RegisterSenderFactory(ISenderFactory factory)
        {
            _senderFactories[factory.FactoryName] = factory;
            return this;
        }

        /// <summary>
        /// Resolves a sender by passing a <see cref="Configuration.SenderConfiguration"/> from Environment down to the
        /// <see cref="ISenderFactory"/>. The factory is loaded either based on the value from the environment variable
        /// <see cref="Configuration.JaegerSenderFactory"/> or, in its absence or failure to deliver a <see cref="ISender"/>,
        /// via reflection. If no factories are found, a <see cref="NoopSender"/> is returned. If multiple factories
        /// are available, the factory whose <see cref="ISenderFactory.FactoryName"/> matches the JAEGER_SENDER_FACTORY env var is
        /// selected. If none matches, <see cref="NoopSender"/> is returned.
        /// </summary>
        /// <returns>The resolved <see cref="ISender"/>, or <see cref="NoopSender"/></returns>
        public ISender Resolve()
        {
            return Resolve(Configuration.SenderConfiguration.FromEnv(_loggerFactory));
        }

        /// <summary>
        /// Resolves a sender by passing the given <see cref="Configuration.SenderConfiguration"/> down to the
        /// <see cref="ISenderFactory"/>. The factory is loaded either based on the value from the environment variable
        /// <see cref="Configuration.JaegerSenderFactory"/> or, in its absence or failure to deliver a <see cref="ISender"/>,
        /// via reflection. If no factories are found, a <see cref="NoopSender"/> is returned. If multiple factories
        /// are available, the factory whose <see cref="ISenderFactory.FactoryName"/> matches the JAEGER_SENDER_FACTORY env var is
        /// selected. If none matches, <see cref="NoopSender"/> is returned.
        /// </summary>
        /// <param name="senderConfiguration">The configuration to pass down to the factory</param>
        /// <returns>The resolved <see cref="ISender"/>, or <see cref="NoopSender"/></returns>
        public ISender Resolve(Configuration.SenderConfiguration senderConfiguration)
        {
            var senderFactory = GetSenderFactory(senderConfiguration.SenderFactory);
            if (senderFactory != null)
            {
                return GetSenderFromFactory(senderFactory, senderConfiguration);
            }

            _logger.LogWarning("No suitable sender found. Using NoopSender, meaning that data will not be sent anywhere!");
            return NoopSender.Instance;
        }

        private ISenderFactory GetSenderFactory(string requestedFactoryName)
        {
            if (_senderFactories.Count > 1)
            {
                _logger.LogDebug("There are multiple sender factories available via reflection.");
            }

            if (requestedFactoryName != null)
            {
                if (_senderFactories.TryGetValue(requestedFactoryName, out var senderFactory))
                {
                    _logger.LogDebug($"Found the requested ({requestedFactoryName}) sender factory: " + senderFactory);
                }

                return senderFactory;
            }

            if (_senderFactories.Count == 1)
            {
                return _senderFactories.First().Value;
            }

            return null;
        }

        private ISender GetSenderFromFactory(ISenderFactory senderFactory, Configuration.SenderConfiguration configuration)
        {
            try
            {
                var sender = senderFactory.GetSender(_loggerFactory, configuration);
                _logger.LogDebug($"Using sender {sender}");
                return sender;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Failed to get a sender from the sender factory.", e);
                return null;
            }
        }
    }
}