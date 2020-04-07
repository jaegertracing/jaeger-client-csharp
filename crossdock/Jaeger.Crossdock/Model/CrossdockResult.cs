using System;
using System.Text.Json.Serialization;

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

        [JsonPropertyName("status")]
        public string Status { get; }

        [JsonPropertyName("output")]
        public string Output { get; }

        public CrossdockResult()
        {
        }

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
