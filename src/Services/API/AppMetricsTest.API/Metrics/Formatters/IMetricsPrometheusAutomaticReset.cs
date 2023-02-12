using App.Metrics.Formatters;
using System;

namespace AppMetricsTest.API.Metrics.Formatters
{
    public interface IMetricsPrometheusAutomaticReset : IMetricsOutputFormatter 
    {
        public DateTime? DateTimeReset { get; }        
    }
}
