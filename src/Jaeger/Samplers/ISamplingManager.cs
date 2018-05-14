using System.Threading.Tasks;
using Jaeger.Samplers.HTTP;

namespace Jaeger.Samplers
{
    public interface ISamplingManager
    {
        Task<SamplingStrategyResponse> GetSamplingStrategyAsync(string serviceName);
    }
}
