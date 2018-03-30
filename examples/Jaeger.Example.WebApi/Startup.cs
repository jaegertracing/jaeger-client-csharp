using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jaeger.Example.WebApi
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
            services.AddTransient<ISampler, ConstSampler>(ctx => new ConstSampler(true));
            services.AddTransient<IReporter, LoggingReporter>();
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
