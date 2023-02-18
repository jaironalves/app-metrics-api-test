using App.Metrics;
using App.Metrics.AspNetCore;
using AppMetricsTest.API.Metrics.Formatters;
using AppMetricsTest.API.Metrics.Options.Configure;
using AppMetricsTest.API.Metrics.StartupFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AppMetricsTest.API.Metrics.Extensions
{
    public static class MetricsHostBuilderExtensions
    {
        private static bool metricsBuilt;

        public static IHostBuilder UsePrometheusMetrics(this IHostBuilder hostBuilder)
        {
            //if (!metricsBuilt)
            //{
            //    hostBuilder
            //        .ConfigureMetricsWithDefaults((HostBuilderContext context, IMetricsBuilder metricsBuilder) =>
            //        {
            //            metricsBuilder
            //                .OutputMetrics.AsPrometheusPlainText();
            //        });
            //    metricsBuilt = true;
            //};

            hostBuilder            
            .ConfigureServices(services =>
            {
                var metricsRoot = AppMetrics.CreateDefaultBuilder()
                    .OutputMetrics.AsPrometheusPlainText()
                    .Build();

                services.AddMetrics(metricsRoot);

                services.AddSingleton<IMetricsPrometheusAutomaticReset, MetricsPrometheusAutomaticReset>();
                services.ConfigureOptions<MetricCustomOptionsSetup>();
                services.ConfigureOptions<MetricEndpointsOptionsSetup>();

            })
            .UseMetrics<AppMetricsStartupFilter>();

            return hostBuilder;
        }
    }
}
