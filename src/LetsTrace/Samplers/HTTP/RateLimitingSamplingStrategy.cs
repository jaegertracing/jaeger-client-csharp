namespace Jaeger.Core.Samplers.HTTP
{
    public class RateLimitingSamplingStrategy
    {
        public short MaxTracesPerSecond { get; set; }
    }
}