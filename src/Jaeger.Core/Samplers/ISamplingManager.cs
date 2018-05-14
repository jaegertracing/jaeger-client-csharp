using System.Threading.Tasks;
using Jaeger.Core.Samplers.HTTP;

namespace Jaeger.Core.Samplers
{
    public interface ISamplingManager
    {
        Task<SamplingStrategyResponse> GetSamplingStrategyAsync(string serviceName);
    }
}
