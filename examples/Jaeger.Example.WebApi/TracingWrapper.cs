using Jaeger.Core;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Microsoft.AspNetCore.Hosting;

namespace Jaeger.Example.WebApi
{
    public interface ITracingWrapper
    {
        IJaegerCoreTracer GetTracer();
    }

    public class TracingWrapper : ITracingWrapper
    {
        private IJaegerCoreTracer _tracer;
        public TracingWrapper(IHostingEnvironment env, IReporter reporter, ISampler sampler) {
            _tracer = new Tracer.Builder(env.ApplicationName)
                .WithReporter(reporter)
                .WithSampler(sampler)
                .Build();
        }

        public IJaegerCoreTracer GetTracer() => _tracer;
    }
}
