using LetsTrace.Reporters;
using Microsoft.AspNetCore.Hosting;

namespace LetsTrace.Example.WebApi
{
    public interface ITracingWrapper
    {
        ILetsTraceTracer GetTracer();
    }

    public class TracingWrapper : ITracingWrapper
    {
        private ILetsTraceTracer _tracer;
        public TracingWrapper(IHostingEnvironment env, IReporter reporter) {
            _tracer = new Tracer(env.ApplicationName, reporter, "192.168.1.1");
        }

        public ILetsTraceTracer GetTracer() => _tracer;
    }
}
