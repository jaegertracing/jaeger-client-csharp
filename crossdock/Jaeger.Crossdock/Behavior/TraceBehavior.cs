using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jaeger.Crossdock.Model;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace Jaeger.Crossdock.Behavior
{
    public class TraceBehavior
    {
        private readonly ITracer _tracer;
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        public TraceBehavior(HttpClient client, ITracer tracer, ILogger logger)
        {
            _client = client;
            _tracer = tracer;
            _logger = logger;
        }

        public async Task<TraceResponse> PrepareResponseAsync(Downstream downstream)
        {
            var response = new TraceResponse(ObserveSpan());

            if (downstream != null)
            {
                var downstreamResponse = await CallDownstreamAsync(downstream);
                response.Downstream = downstreamResponse;
            }

            return response;
        }

        private Task<TraceResponse> CallDownstreamAsync(Downstream downstream)
        {
            _logger.LogInformation("Calling downstream {downstream}", downstream);
            _logger.LogInformation(
                "Downstream service {serviceName} -> {host}:{port}",
                downstream.ServiceName,
                downstream.Host,
                downstream.Port);

            var transport = downstream.Transport;
            switch (transport)
            {
                case Constants.TRANSPORT_HTTP:
                    return CallDownstreamHttpAsync(downstream);
                default:
                    return Task.FromResult(new TraceResponse("Unrecognized transport received: " + transport));
            }
        }

        private async Task<TraceResponse> CallDownstreamHttpAsync(Downstream downstream)
        {
            var downstreamUrl = $"http://{downstream.Host}:{downstream.Port}/join_trace";
            _logger.LogInformation("Calling downstream http {serviceName} at {downstream}", downstream.ServiceName, downstreamUrl);

            var jsonContent = JsonSerializer.Serialize(new JoinTraceRequest(downstream.ServerRole, downstream.Downstream_));
            var resp = await _client.PostAsync(downstreamUrl, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Received response with status code {statusCode}", resp.StatusCode);
                return null;
            }
            var response = await JsonSerializer.DeserializeAsync<TraceResponse>(await resp.Content.ReadAsStreamAsync());
            _logger.LogInformation("Received response {response}", response);
            return response;
        }

        private ObservedSpan ObserveSpan()
        {
            var span = (Span)_tracer.ActiveSpan;
            if (span == null)
            {
                _logger.LogError("No span found");
                return new ObservedSpan("no span found", false, "no span found");
            }

            var context = span.Context;
            var traceId = context.TraceId.ToString();
            var sampled = context.IsSampled;
            var baggage = span.GetBaggageItem(Constants.BAGGAGE_KEY);
            return new ObservedSpan(traceId, sampled, baggage);
        }
    }
}