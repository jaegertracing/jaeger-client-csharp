using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jaeger.Crossdock.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jaeger.Crossdock.Controllers
{
    [Produces("application/json")]
    [Route("")]
    [ApiController]
    public class CrossdockController : ControllerBase
    {
        private const string TRANSPORT_HTTP = "http";
        private const string TRANSPORT_DUMMY = "dummy";

        private static readonly Random Random = new Random();
        private static readonly byte[] Buffer = new byte[8];

        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public CrossdockController(HttpClient client, ILogger<EndToEndBehaviorController> logger)
        {
            _client = client;
            _logger = logger;
        }

        [HttpHead]
        public ActionResult Health()
        {
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> Client(string behavior, string client, string s1name, string s2name, string s2transport, string s3name, string s3transport,
            bool sampled)
        {
            if (behavior != "trace") return ResultFromCrossdock(CrossdockResult.Result.Error, $"Unknown behavior: {behavior}");
            if (client != "csharp") return ResultFromCrossdock(CrossdockResult.Result.Error, $"Unknown client: {client}");
            if (string.IsNullOrEmpty(s1name)) return ResultFromCrossdock(CrossdockResult.Result.Error, $"Value must not be null or empty: {s1name}");
            if (string.IsNullOrEmpty(s2name)) return ResultFromCrossdock(CrossdockResult.Result.Error, $"Value must not be null or empty: {s2name}");
            if (string.IsNullOrEmpty(s2transport)) return ResultFromCrossdock(CrossdockResult.Result.Error, $"Value must not be null or empty: {s2transport}");
            if (string.IsNullOrEmpty(s3name)) return ResultFromCrossdock(CrossdockResult.Result.Error, $"Value must not be null or empty: {s3name}");
            if (string.IsNullOrEmpty(s3transport)) return ResultFromCrossdock(CrossdockResult.Result.Error, $"Value must not be null or empty: {s3transport}");

            var baggage = RandomBaggage();
            var level3 = new Downstream(s3name, s3name, TransportToPort(s3transport), StringToTransport(s3transport), "S3", null);
            var level2 = new Downstream(s2name, s2name, TransportToPort(s2transport), StringToTransport(s2transport), "S2", level3);
            var level1 = new StartTraceRequest("S1", sampled, baggage, level2);

            var response = await StartTraceAsync(s1name, level1);
            if (response == null || !string.IsNullOrEmpty(response.NotImplementedError))
            {
                return ResultFromCrossdock(CrossdockResult.Result.Error, response?.NotImplementedError);
            }

            for (var r = response; r != null; r = r.Downstream)
            {
                if (!string.IsNullOrEmpty(r.NotImplementedError))
                {
                    _logger.LogInformation("SKIP: {reason}", r.NotImplementedError);
                    return ResultFromCrossdock(CrossdockResult.Result.Skip, r.NotImplementedError);
                }
            }

            var traceID = response.Span.TraceId;
            if (string.IsNullOrEmpty(traceID))
            {
                return ResultFromCrossdock(CrossdockResult.Result.Error, $"Trace ID is empty in S1({s1name})");
            }

            var result = ValidateTrace(level1.Downstream, response, s1name, 1, traceID, sampled, baggage);
            if (result != null) return ResultFromCrossdock(CrossdockResult.Result.Error, result);

            _logger.LogInformation("PASS");
            return ResultFromCrossdock(CrossdockResult.Result.Success, "trace checks out");
        }

        private ActionResult ResultFromCrossdock(CrossdockResult.Result result, string output = null)
        {
            return Ok(new CrossdockResult[]{
                new CrossdockResult(result, output)
            });
        }

        private string ValidateTrace(Downstream target, TraceResponse resp, string service, int level, string traceID, bool sampled, string baggage)
        {
            if (traceID != resp.Span.TraceId)
            {
                return $"Trace ID mismatch in S{level}({service}): expected {traceID}, received {resp.Span.TraceId}";
            }
            if (baggage != resp.Span.Baggage)
            {
                return $"Baggage mismatch in S{level}({service}): expected {baggage}, received {resp.Span.Baggage}";
            }
            if (sampled != resp.Span.Sampled)
            {
                return $"Sampled mismatch in S{level}({service}): expected {sampled}, received {resp.Span.Sampled}";
            }
            if (target != null)
            {
                if (resp.Downstream == null)
                {
                    return $"Missing downstream in S{level}({service})";
                }

                return ValidateTrace(target.Downstream_, resp.Downstream, target.Host, level + 1, traceID, sampled, baggage);
            }

            if (resp.Downstream != null)
            {
                return $"Unexpected downstream in S{level}({service})";
            }

            return null;
        }

        private async Task<TraceResponse> StartTraceAsync(string host, StartTraceRequest request)
        {
            var url = $"http://{host}:{Constants.DEFAULT_SERVER_PORT_HTTP}/start_trace";
            _logger.LogInformation("Calling start_trace on {serviceName}", host);

            var resp = await _client.PostAsJsonAsync(url, request);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Received response with status code {statusCode}", resp.StatusCode);
                return new TraceResponse(await resp.Content.ReadAsStringAsync());
            }
            var response = await resp.Content.ReadAsAsync<TraceResponse>();
            _logger.LogInformation("Received response {response}", response);
            return response;
        }

        private static string TransportToPort(string transport)
        {
            switch (transport)
            {
                case TRANSPORT_HTTP:
                    return Constants.DEFAULT_SERVER_PORT_HTTP;
                case TRANSPORT_DUMMY:
                    return "9999";
                default:
                    throw new ArgumentOutOfRangeException(nameof(transport), transport, null);
            }
        }

        private static string StringToTransport(string transport)
        {
            switch (transport)
            {
                case TRANSPORT_HTTP:
                    return Constants.TRANSPORT_HTTP;
                case TRANSPORT_DUMMY:
                    return Constants.TRANSPORT_DUMMY;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transport), transport, null);
            }
        }

        private static string RandomBaggage()
        {
            Random.NextBytes(Buffer);
            var u = BitConverter.ToUInt64(Buffer, 0);
            return u.ToString("x");
        }
    }
}
