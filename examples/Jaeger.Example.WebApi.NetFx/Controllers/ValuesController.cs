using OpenTracing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Http;

namespace Jaeger.Example.WebApi.NetFx.Controllers
{
    [RoutePrefix("values")]
    public class ValuesController : ApiController
    {
        private readonly ITracer _tracer;

        public ValuesController(ITracer tracer)
        {
            _tracer = tracer;
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<string> GetAll()
        {
            using (var scope1 = _tracer.BuildSpan("Controller-GetAll").StartActive(true))
            {
                using (var scope2 = _tracer.BuildSpan("DataSource-GetAll").StartActive(true))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    return new string[] { "value1", "value2" };
                }
            }
        }
    }
}
