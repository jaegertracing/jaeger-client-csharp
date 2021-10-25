using Autofac;
using Autofac.Integration.WebApi;
using Jaeger.Samplers;
using OpenTracing;
using System.Reflection;
using System.Web.Http;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.Extensions.Logging;

namespace Jaeger.Example.WebApi.NetFx
{
    public class AutofacConfig
    {
        private static readonly Tracer tracer;

        static AutofacConfig()
        {
            // This is necessary to pick the correct sender, otherwise a NoopSender is used!
            Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(null)
                .RegisterSenderFactory<ThriftSenderFactory>();

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