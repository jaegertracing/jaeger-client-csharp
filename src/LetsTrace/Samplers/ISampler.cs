using System;
using System.Collections.Generic;

namespace LetsTrace.Samplers
{
    public interface ISampler : IDisposable
    {
        // IsSampled decides whether a trace with given `id` and `operation`
        // should be sampled. This function will also return the tags that
        // can be used to identify the type of sampling that was applied to
        // the root span. Most simple samplers would return two tags,
        // sampler.type and sampler.param, similar to those used in the Configuration
        (bool Sampled, Dictionary<string, object> Tags) IsSampled(TraceId id, string operation);
    }
}
