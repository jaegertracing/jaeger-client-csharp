using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsTrace.Reporters;
using LetsTrace.Transport;
using LetsTrace.Transport.Zipkin.ZipkinJSON;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetsTrace.Example.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging(builder => {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
            });
            services.AddTransient<ITransport, ZipkinJSONTransport>(ctx => new ZipkinJSONTransport(new Uri("http://localhost:9411/api/v2/spans"), 0));
            services.AddTransient<IReporter, RemoteReporter>();
            services.AddTransient<ITracingWrapper, TracingWrapper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<TracingMiddleware>();

            app.UseMvc();
        }
    }
}
