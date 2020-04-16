using System.Net.Http;
using System.Threading.Tasks;
using Jaeger.Crossdock.Behavior;
using Jaeger.Crossdock.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;

namespace Jaeger.Crossdock.Controllers
{
    [Produces("application/json")]
    [Route("")]
    [ApiController]
    public class TraceBehaviorController : ControllerBase
    {
        private readonly ITracer _tracer;
        private readonly ILogger _logger;
        private readonly TraceBehavior _behavior;

        public TraceBehaviorController(HttpClient client, ITracer tracer, ILogger<EndToEndBehaviorController> logger)
        {
            _tracer = tracer;
            _logger = logger;
            _behavior = new TraceBehavior(client, _tracer, logger);
        }

        [Route("start_trace")]
        [HttpPost]
        public async Task<ActionResult<TraceResponse>> StartTrace(StartTraceRequest startRequest)
        {
            using (var scope = _tracer
                .BuildSpan("start_trace")
                .IgnoreActiveSpan()
                .StartActive())
            {
                _logger.LogInformation("http:start_trace request: {startRequest}", startRequest);
                var baggage = startRequest.Baggage;
                var span = scope.Span;
                span.SetBaggageItem(Constants.BAGGAGE_KEY, baggage);
                if (startRequest.Sampled)
                {
                    Tags.SamplingPriority.Set(span, 1);
                }

                var response = await _behavior.PrepareResponseAsync(startRequest.Downstream);
                _logger.LogInformation("http:start_trace response: {response}", response);
                return response;
            }
        }

        [Route("join_trace")]
        [HttpPost]
        public async Task<ActionResult<TraceResponse>> JoinTrace(JoinTraceRequest joinRequest)
        {
            _logger.LogInformation("http:join_trace request: {joinRequest}", joinRequest);
            var response = await _behavior.PrepareResponseAsync(joinRequest.Downstream);
            _logger.LogInformation("http:join_trace response: {response}", response);
            return response;
        }
    }
}
