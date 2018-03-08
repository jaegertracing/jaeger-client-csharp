namespace LetsTrace.Samplers
{
    // ISamplerFactory defines a factory to build new samplers
    internal interface ISamplerFactory
    {
        IGuaranteedThroughputProbabilisticSampler NewGuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound);
        IProbabilisticSampler NewProbabilisticSampler(double samplingRate);
    }
}
