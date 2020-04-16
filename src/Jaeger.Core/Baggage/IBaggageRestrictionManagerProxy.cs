using System.Collections.Generic;
using System.Threading.Tasks;
using Jaeger.Baggage.Http;

namespace Jaeger.Baggage
{
    /// <summary>
    /// <see cref="IBaggageRestrictionManagerProxy"/> is an interface for a class that fetches baggage
    /// restrictions for specific service from a remote source i.e. jaeger-agent.
    /// </summary>
    public interface IBaggageRestrictionManagerProxy
    {
        Task<List<BaggageRestrictionResponse>> GetBaggageRestrictionsAsync(string serviceName);
    }
}