namespace LetsTrace.Samplers
{
    // SamplerFactory is a factory to build new samplers
    internal class SamplerFactory : ISamplerFactory
    {
        public ISampler NewGuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound) => new GuaranteedThroughputProbabilisticSampler(samplingRate, lowerBound);

        public ISampler NewProbabilisticSampler(double samplingRate) => new ProbabilisticSampler(samplingRate);
    }
}
