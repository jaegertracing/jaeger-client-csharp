using System;
using Jaeger.Core.Samplers.HTTP;

namespace Jaeger.Core.Samplers
{
    public interface ISamplingManager
    {
        SamplingStrategyResponse GetSamplingStrategy(String serviceName);
    }
}
