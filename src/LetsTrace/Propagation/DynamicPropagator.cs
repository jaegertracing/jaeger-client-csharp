using OpenTracing;
using OpenTracing.Propagation;
using System;
using System.Collections.Generic;

namespace LetsTrace.Propagation
{
    class DynamicPropagator : IPropagator
    {
        internal Dictionary<string, IInjector> _injectors { get; } = new Dictionary<string, IInjector>();
        internal Dictionary<string, IExtractor> _extractors { get; } = new Dictionary<string, IExtractor>();

        public void AddCodec<TCarrier>(IFormat<TCarrier> format, IInjector injector, IExtractor extractor)
        {
            var formatString = format.ToString();
            if (!_injectors.ContainsKey(formatString))
            {
                _injectors.Add(formatString, injector);
            }

            if (!_extractors.ContainsKey(formatString))
            {
                _extractors.Add(formatString, extractor);
            }
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            var formatString = format.ToString();
            if (_extractors.ContainsKey(formatString))
            {
                return _extractors[formatString].Extract(carrier);
            }
            throw new Exception($"{format} is not a supported extraction format");
        }


        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            var formatString = format.ToString();
            if (_injectors.ContainsKey(formatString))
            {
                _injectors[formatString].Inject(spanContext, carrier);
                return;
            }
            throw new Exception($"{format} is not a supported injection format");
        }
    }
}
