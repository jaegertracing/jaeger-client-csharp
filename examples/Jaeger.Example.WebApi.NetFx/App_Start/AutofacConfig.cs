using Autofac;
using Autofac.Integration.WebApi;
using Jaeger.Samplers;
using OpenTracing;
using System.Reflection;
using System.Web.Http;

namespace Jaeger.Example.WebApi.NetFx
{
    public class AutofacConfig
    {
        private static readonly Tracer tracer;

        static AutofacConfig()
        {
            tracer = new Tracer.Builder("WebApi-NetFx")
                .WithSampler(new ConstSampler(true))
                .Build();
        }

        public static void Register(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterInstance(tracer).As<ITracer>();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
        }
    }
}