using App.Metrics.Formatters.Prometheus;
using App.Metrics.Formatters;
using App.Metrics;
using AppMetricsTest.API.Metrics.Options;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using AppMetricsTest.API.Provider;

namespace AppMetricsTest.API.Metrics.Formatters
{
    public class MetricsPrometheusAutomaticReset : IMetricsPrometheusAutomaticReset
    {        
        private readonly IMetrics metrics;
        private readonly MetricCustomOptions options;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly MetricsPrometheusTextOutputFormatter metricsPrometheus;

        public DateTime? DateTimeReset { get; private set; } = null;

        public MetricsPrometheusAutomaticReset(IMetricsRoot metrics, IOptions<MetricCustomOptions> options, IDateTimeProvider dateTimeProvider)
        {            
            this.metrics = metrics;
            this.options = options.Value;
            this.dateTimeProvider = dateTimeProvider;
            metricsPrometheus = new MetricsPrometheusTextOutputFormatter();
        }

        public MetricsMediaTypeValue MediaType => metricsPrometheus.MediaType;

        public MetricFields MetricFields { get => metricsPrometheus.MetricFields; set => metricsPrometheus.MetricFields = value; }

        private void ClearContexts()
        {
            metrics.Manage.ShutdownContext(options.ContextNameReset);
        }

        private void CheckClearContextsInterval()
        {
            var timeSpanInterval = TimeSpan.FromSeconds(120);

            if (!DateTimeReset.HasValue)
            {
                DateTimeReset = dateTimeProvider.Now.Add(timeSpanInterval);
                return;
            }

            if (dateTimeProvider.Now > DateTimeReset)
            {
                var dateTimeResetNext = DateTimeReset?.Add(timeSpanInterval);
                if (dateTimeProvider.Now > dateTimeResetNext)
                {
                    DateTimeReset = null;
                    return;
                }

                DateTimeReset = null;
                ClearContexts();
            }
        }

        public async Task WriteAsync(Stream output, MetricsDataValueSource metricsData, CancellationToken cancellationToken = default)
        {
            await metricsPrometheus.WriteAsync(output, metricsData, cancellationToken);                        
            CheckClearContextsInterval();




            //var metricsContextValueSource02 = metrics.Snapshot.GetForContext("Custom.Requests");

            ////var firt = metricsRoot.Reporters.First();
            //var value = metricsContextValueSource02.BucketTimers.FirstOrDefault();
            //if (value != null)
            //{

            //}
        }

    }
}
