using System;
using Newtonsoft.Json;

namespace Jaeger.Crossdock.Model
{
    public class CrossdockResult
    {
        public enum Result
        {
            Skip,
            Error,
            Success
        }

        [JsonProperty("status")]
        public string Status { get; }

        [JsonProperty("output")]
        public string Output { get; }

        [JsonConstructor]
        public CrossdockResult(Result result, string output = null)
        {
            Output = output;

            switch (result)
            {
                case Result.Skip:
                    Status = "skipped";
                    break;

                case Result.Error:
                    Status = "failed";
                    break;

                case Result.Success:
                    Status = "passed";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
