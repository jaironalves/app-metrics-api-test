using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.Filtering;
using App.Metrics.Filters;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Reporting;
using AppMetricsTest.API.Metrics.Extensions;
using AppMetricsTest.API.Metrics.Formatters;
using AppMetricsTest.API.Metrics.Options.Configure;
using AppMetricsTest.API.Metrics.StartupFilter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsTest.API
{    
    

    


    
   


    public class CustomMetricsResponseWriter : IMetricsResponseWriter
    {
        private readonly IMetrics metrics;

        public CustomMetricsResponseWriter(IMetrics metrics)
        {
            this.metrics = metrics;
        }

        public Task WriteAsync(HttpContext context, MetricsDataValueSource metricsData, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }

    public class ReporterMetricsCustom : IReportMetrics
    {
        public IFilterMetrics Filter { get; set; } = new MetricsFilter().WhereContext(c => true);
        public TimeSpan FlushInterval { get; set; } = AppMetricsConstants.Reporting.DefaultFlushInterval;
        public IMetricsOutputFormatter Formatter { get; set; }

        public Task<bool> FlushAsync(MetricsDataValueSource metricsData, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Aqui");
            return Task.FromResult(true);
        }
    }


    public class Program
    {        
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //var Metrics = AppMetrics.CreateDefaultBuilder()
            //    //.OutputMetrics.AsPrometheusPlainText()
            //    //.Report.Using<ReporterMetricsCustom>()
            //    .Build();

            return Host.CreateDefaultBuilder(args)
                //.UseMetricsEndpoints()
                //.UseMetricsWebTracking()
                .ConfigureLogging(cfLg =>
                {
                    
                })
                .ConfigureServices(s =>
                {
                    s.AddHttpContextAccessor();
                })
                //.ConfigureServices(s =>
                //{
                //    //s.AddSingleton<IConfigureOptions<MetricEndpointsOptions>, MetricsCustomEndpointsOptionsSetup>();
                //    //s.AddSingleton<IPostConfigureOptions<MetricEndpointsOptions>, MetricsCustomOptionsSetup>();

                //    s.AddSingleton<IMetricsPrometheusAutomaticReset, MetricsPrometheusAutomaticReset>();

                //    s.ConfigureOptions<MetricCustomOptionsSetup>();
                //    s.ConfigureOptions<MetricEndpointsOptionsSetup>();

                //    //s.AddSingleton(sp =>
                //    //{
                //    //    var opt = new MetricCustomOptions()
                //    //    {
                //    //        ContextName = "test",
                //    //        ContextNameReset = "Custom.Requests"
                //    //    };

                //    //    return opt;
                //    //});

                //    //s.AddSingleton<CustomMetricsResponseWriter>();                    
                //    //s.AddOptions<MetricsWebHostOptions>()
                //    //    .Configure<IMetricsOutputFormatter>((o, opt) => o.EndpointOptions = (edp) =>
                //    //    {
                //    //        edp.MetricsEndpointOutputFormatter = opt;
                //    //    });
                //})
                .UsePrometheusMetrics()
                //.UseMetrics()
                //.UseMetrics(
                //(HostBuilderContext context, MetricsWebHostOptions options) =>
                //{
                //    options.EndpointOptions = endpointsOptions =>
                //    {
                //        endpointsOptions.MetricsEndpointEnabled = endpointsOptions.MetricsEndpointEnabled;

                //        //endpointsOptions.
                //        //endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                //        //endpointsOptions.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatterAutomaticReset();
                //    };
                //}
                //           )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
