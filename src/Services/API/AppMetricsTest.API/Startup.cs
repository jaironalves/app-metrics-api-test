using AppMetricsTest.API.Metrics;
using AppMetricsTest.API.Metrics.HttpClient;
using AppMetricsTest.API.Metrics.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppMetricsTest.API
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
            services.AddSingleton<IRequestAccessor, RequestAccessor>();

            services.AddHttpClient("post")
                .AddHttpMessageHandler<RequestDelegatingHandler>()
                .AddHttpMessageHandler<RequestErrorMeterHandler>()
                .AddHttpMessageHandler<RequestInProgressHandler>()
                .AddHttpMessageHandler<RequestTimerHandler>();
                
                

            services.AddTransient<RequestDelegatingHandler>();
            services.AddTransient<RequestTimerHandler>();
            services.AddTransient<RequestInProgressHandler>();
            services.AddTransient<RequestErrorMeterHandler>();
            

            services.AddControllers(
               // cfg => cfg.Filters.Add<>()
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<RequestMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
