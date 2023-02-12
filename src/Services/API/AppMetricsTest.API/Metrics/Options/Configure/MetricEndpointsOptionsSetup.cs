using App.Metrics.AspNetCore.Endpoints;
using AppMetricsTest.API.Metrics.Formatters;
using Microsoft.Extensions.Options;

namespace AppMetricsTest.API.Metrics.Options.Configure
{
    public class MetricEndpointsOptionsSetup :
        IPostConfigureOptions<MetricEndpointsOptions>
    {
        //private readonly IMetricsPrometheusAutomaticReset metricsPrometheusAutomaticReset;

        //public MetricEndpointsOptionsSetup(IMetricsPrometheusAutomaticReset metricsPrometheusAutomaticReset)
        //{
        //    this.metricsPrometheusAutomaticReset = metricsPrometheusAutomaticReset;
        //}

        public MetricEndpointsOptionsSetup()
        {

        }

        public void PostConfigure(string name, MetricEndpointsOptions options)
        {
            options.MetricsEndpointEnabled = options.MetricsEndpointEnabled;
            //options.MetricsEndpointOutputFormatter = metricsPrometheusAutomaticReset;
        }
    }
}
