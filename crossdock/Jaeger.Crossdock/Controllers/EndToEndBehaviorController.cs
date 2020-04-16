using Jaeger.Crossdock.Behavior;
using Jaeger.Crossdock.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jaeger.Crossdock.Controllers
{
    [Produces("application/json")]
    [Route("")]
    [ApiController]
    public class EndToEndBehaviorController : ControllerBase
    {
        private readonly EndToEndBehavior _behavior;
        private readonly ILogger _logger;

        public EndToEndBehaviorController(EndToEndBehavior behavior, ILogger<EndToEndBehaviorController> logger)
        {
            _behavior = behavior;
            _logger = logger;
        }

        [Route("create_traces")]
        [HttpPost]
        public ActionResult CreateTraces(CreateTracesRequest request)
        {
            _logger.LogInformation("http:create_traces request: {request}", request);
            _behavior.GenerateTraces(request);
            return Ok("OK");
        }
    }
}
