using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;

namespace LetsTrace.Example.WebApi
{
    public class TracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITracingWrapper _tracer;
        private readonly ILogger<TracingMiddleware> _logger;

        public TracingMiddleware(RequestDelegate next, ITracingWrapper tracer, ILogger<TracingMiddleware> logger)
        {
            _next = next;
            _tracer = tracer;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var operationName = $"{context.Request.Method.ToUpper()}{context.Request.Path}";
            _logger.LogInformation($"Starting a new span: {operationName}");
            
            var builder = _tracer.GetTracer().BuildSpan(operationName)
                .WithTag(Tags.SpanKind.Key, Tags.SpanKindServer);

            using ((ILetsTraceSpan)builder.Start())
            {
                await _next(context);
                _logger.LogInformation($"Finishing span: {operationName}");
            }
        }
    }

}
