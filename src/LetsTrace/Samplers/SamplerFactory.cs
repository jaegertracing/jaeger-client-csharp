using Microsoft.Extensions.Logging;

namespace Jaeger.Core.Samplers
{
    // SamplerFactory is a factory to build new samplers
    internal class SamplerFactory : ISamplerFactory
    {
        public ISampler NewGuaranteedThroughputProbabilisticSampler(double samplingRate, double lowerBound) => new GuaranteedThroughputProbabilisticSampler(samplingRate, lowerBound);

        public ISampler NewProbabilisticSampler(double samplingRate) => new ProbabilisticSampler(samplingRate);

        public ISampler NewRateLimitingSampler(short maxTracesPerSecond) => new RateLimitingSampler(maxTracesPerSecond);

        public ISampler NewPerOperationSampler(int maxOperations, double samplingRate, double lowerBound, ILoggerFactory loggerFactory) => new PerOperationSampler(maxOperations, samplingRate, lowerBound, loggerFactory);
    }
}
