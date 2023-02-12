using Microsoft.Extensions.Options;
using System;

namespace AppMetricsTest.API.Metrics.Options.Configure
{
    public class MetricCustomOptionsSetup :
        IPostConfigureOptions<MetricCustomOptions>
    {
        private readonly IServiceProvider serviceProvider;

        public MetricCustomOptionsSetup(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void PostConfigure(string name, MetricCustomOptions options)
        {
            options.Update(options.ContextName, "Custom.Requests");
        }

    }
}
