using Microsoft.Extensions.Logging;

namespace LetsTrace.Samplers
{
    // ISamplerFactory defines a factory to build new samplers
    internal interface ISamplerFactory
    {
        ISampler NewGuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound);
        ISampler NewProbabilisticSampler(double samplingRate);
        ISampler NewRateLimitingSampler(short maxTracesPerSecond);
        ISampler NewPerOperationSampler(int maxOperations, double samplingRate, double lowerBound, ILoggerFactory loggerFactory);
    }
}
