using OpenTracing;
using OpenTracing.Propagation;
using System;
using System.Collections.Generic;

namespace LetsTrace.Propagation
{
    public class PropagationRegistry : IPropagationRegistry
    {
        internal readonly Dictionary<string, IInjector> _injectors = new Dictionary<string, IInjector>();
        internal readonly Dictionary<string, IExtractor> _extractors = new Dictionary<string, IExtractor>();

        public void AddCodec<TCarrier>(IFormat<TCarrier> format, IInjector injector, IExtractor extractor)
        {
            var formatString = format.ToString();
            _injectors[formatString] = injector;
            _extractors[formatString] = extractor;
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            var formatString = format.ToString();
            if (_extractors.ContainsKey(formatString))
            {
                return _extractors[formatString].Extract(carrier);
            }

            throw new ArgumentException($"{formatString} is not a supported extraction format", nameof(format));
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            var formatString = format.ToString();
            if (_injectors.ContainsKey(formatString))
            {
                _injectors[formatString].Inject(spanContext, carrier);
                return;
            }

            throw new ArgumentException($"{formatString} is not a supported injection format", nameof(format));
        }
    }
}
