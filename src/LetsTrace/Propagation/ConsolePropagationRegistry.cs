using System;
using OpenTracing;
using OpenTracing.Propagation;

namespace LetsTrace.Propagation
{
    public sealed class ConsolePropagationRegistry : IPropagationRegistry
    {
        // TODO: Use ILogger
        public void AddCodec<TCarrier>(IFormat<TCarrier> format, IInjector injector, IExtractor extractor)
        {
            Console.WriteLine($"AddCodec({format}, {injector}, {extractor}");
        }

        public void Inject<TCarrier>(ISpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            Console.WriteLine($"Inject({context}, {format}, {carrier}");
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            Console.WriteLine($"Extract({format}, {carrier}");
            return null;
        }
    }
}