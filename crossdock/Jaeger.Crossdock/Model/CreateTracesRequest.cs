using System.Collections.Generic;
using Jaeger.Samplers;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class CreateTracesRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; } = RemoteControlledSampler.Type;

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("tags")]
        public IDictionary<string, string> Tags { get; set; }
    }
}
