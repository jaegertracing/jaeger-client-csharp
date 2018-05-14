using System;
using System.Collections.Generic;
using Jaeger.Core.Propagation;
using Microsoft.Extensions.Logging;
using OpenTracing.Propagation;

namespace Jaeger.Core
{
    internal class PropagationRegistry
    {
        private readonly ILogger _logger;
        private readonly Dictionary<object, IInjector> _injectors = new Dictionary<object, IInjector>();
        private readonly Dictionary<object, IExtractor> _extractors = new Dictionary<object, IExtractor>();

        public PropagationRegistry(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<PropagationRegistry>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        internal IInjector GetInjector<TCarrier>(IFormat<TCarrier> format)
        {
            return _injectors.TryGetValue(format, out var injector) ? injector : null;
        }

        internal IExtractor GetExtractor<TCarrier>(IFormat<TCarrier> format)
        {
            return _extractors.TryGetValue(format, out var extractor) ? extractor : null;
        }

        public void Register<TCarrier>(IFormat<TCarrier> format, Injector<TCarrier> injector)
        {
            _injectors[format] = new ExceptionCatchingInjectorDecorator(injector, _logger);
        }

        public void Register<TCarrier>(IFormat<TCarrier> format, Extractor<TCarrier> extractor)
        {
            _extractors[format] = new ExceptionCatchingExtractorDecorator(extractor, _logger);
        }

        public void Register<TCarrier>(IFormat<TCarrier> format, Codec<TCarrier> codec)
        {
            _injectors[format] = new ExceptionCatchingInjectorDecorator(codec, _logger);
            _extractors[format] = new ExceptionCatchingExtractorDecorator(codec, _logger);
        }

        private sealed class ExceptionCatchingExtractorDecorator : IExtractor
        {
            private readonly IExtractor _decorated;
            private readonly ILogger _logger;

            public ExceptionCatchingExtractorDecorator(IExtractor decorated, ILogger logger)
            {
                _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public SpanContext Extract(object carrier)
            {
                try
                {
                    return _decorated.Extract(carrier);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error when extracting SpanContext from carrier. Handling gracefully.");
                    return null;
                }
            }
        }

        private sealed class ExceptionCatchingInjectorDecorator : IInjector
        {
            private readonly IInjector _decorated;
            private readonly ILogger _logger;

            public ExceptionCatchingInjectorDecorator(IInjector decorated, ILogger logger)
            {
                _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public void Inject(SpanContext spanContext, object carrier)
            {
                try
                {
                    _decorated.Inject(spanContext, carrier);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error when injecting SpanContext into carrier. Handling gracefully.");
                }
            }
        }
    }
}
