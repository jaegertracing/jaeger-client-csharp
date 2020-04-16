using System.Net.Http;
using System.Net.Http.Headers;
using Jaeger.Crossdock.Behavior;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace Jaeger.Crossdock
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                var tracer = new Tracer.Builder(Constants.DEFAULT_TRACER_SERVICE_NAME)
                    .WithSampler(new ConstSampler(false))
                    .WithReporter(new LoggingReporter(LoggerFactory))
                    .Build();

                // Allows code that can't use DI to also access the tracer.
                GlobalTracer.Register(tracer);

                return tracer;
            });
            services.AddSingleton(serviceProvider =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

                return new EndToEndBehavior(configuration.GetValue("SAMPLING_HOST_PORT", "jaeger:5778"),
                    Constants.DEFAULT_TRACER_SERVICE_NAME,
                        new UdpSender(configuration.GetValue("AGENT_HOST", "jaeger"), 0, 0));
            });
            services.AddSingleton(serviceProvider =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client;
            });

            services.AddOpenTracing();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}