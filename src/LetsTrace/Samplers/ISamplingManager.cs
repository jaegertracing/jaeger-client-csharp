using System;
using LetsTrace.Samplers.HTTP;

namespace LetsTrace.Samplers
{
    public interface ISamplingManager
    {
        SamplingStrategyResponse GetSamplingStrategy(String serviceName);
    }
}
