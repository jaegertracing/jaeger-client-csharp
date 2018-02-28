namespace LetsTrace.Samplers
{
    // ISamplerFactory defines a factory to build new samplers
    internal interface ISamplerFactory
    {
        ISampler NewGuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound);
        ISampler NewProbabilisticSampler(double samplingRate);
    }
}
