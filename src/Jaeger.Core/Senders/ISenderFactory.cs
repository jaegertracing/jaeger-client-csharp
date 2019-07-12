using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Jaeger.Senders
{
    /// <summary>
    /// Represents a class that knows how to select and build the appropriate <see cref="ISender"/> based on the given
    /// <see cref="Configuration.SenderConfiguration"/>. This factory is usually used in conjunction with the
    /// <see cref="SenderResolver"/>, so that the appropriate factory will be loaded via reflection.
    /// </summary>
    public interface ISenderFactory
    {
        /// <summary>
        /// Builds and/or selects the appropriate sender based on the given <see cref="Configuration.SenderConfiguration"/>
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="senderConfiguration">The sender configuration</param>
        /// <returns>An appropriate sender based on the configuration, or <see cref="NoopSender"/>.</returns>
        ISender GetSender(ILoggerFactory loggerFactory, Configuration.SenderConfiguration senderConfiguration);

        /// <summary>
        /// The Factory's name. Can be specified via <see cref="Configuration.JaegerSenderFactory"/> to disambiguate
        /// the resolution, in case multiple senders are available via reflection.
        /// </summary>
        string FactoryName { get; }
    }
}
