using AppMetricsTest.API.Metrics.Formatters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AppMetricsTest.API.Metrics.StartupFilter
{
    class AppMetricsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return UseAppMetrics;
            void UseAppMetrics(IApplicationBuilder builder)
            {
                var prometheusAutomaticReset = builder
                    .ApplicationServices
                    .GetRequiredService<IMetricsPrometheusAutomaticReset>();

                builder.UseMetricsEndpoint(prometheusAutomaticReset);
                //builder.UseMetricsTextEndpoint();
                //builder.UseEnvInfoEndpoint();
                builder.UseMetricsAllMiddleware();
                next(builder);
            }
        }
    }
}
