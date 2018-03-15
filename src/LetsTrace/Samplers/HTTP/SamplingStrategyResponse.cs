namespace LetsTrace.Samplers.HTTP
{
    public class SamplingStrategyResponse
    {
        public ProbabilisticSamplingStrategy ProbabilisticSampling { get; set; }
        public RateLimitingSamplingStrategy RateLimitingSampling { get; set; }
        public PerOperationSamplingStrategies OperationSampling { get; set; }
    }
}