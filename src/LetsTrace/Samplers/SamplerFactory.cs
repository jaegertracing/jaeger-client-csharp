namespace LetsTrace.Samplers
{
    // SamplerFactory is a factory to build new samplers
    internal class SamplerFactory : ISamplerFactory
    {
        public IGuaranteedThroughputProbabilisticSampler NewGuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound) => new GuaranteedThroughputProbabilisticSampler(samplingRate, lowerBound);

        public IProbabilisticSampler NewProbabilisticSampler(double samplingRate) => new ProbabilisticSampler(samplingRate);
    }
}
