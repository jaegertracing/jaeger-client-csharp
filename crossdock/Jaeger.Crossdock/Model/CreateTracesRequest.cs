using System.Collections.Generic;
using System.Text.Json.Serialization;
using Jaeger.Samplers;

namespace Jaeger.Crossdock.Model
{
    public class CreateTracesRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = RemoteControlledSampler.Type;

        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("tags")]
        public IDictionary<string, string> Tags { get; set; }
    }
}
