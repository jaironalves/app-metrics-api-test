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

namespace AppMetricsTest.API.Metrics.Formatters
{
    public class MetricsPrometheusAutomaticReset : IMetricsPrometheusAutomaticReset
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMetrics metrics;
        private readonly MetricCustomOptions options;

        private readonly MetricsPrometheusTextOutputFormatter metricsPrometheus;

        public DateTime? DateTimeReset { get; private set; } = null;

        public MetricsPrometheusAutomaticReset(IHttpContextAccessor httpContextAccessor, IMetricsRoot metrics, IOptions<MetricCustomOptions> options)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.metrics = metrics;
            this.options = options.Value;
            metricsPrometheus = metrics
                .OutputMetricsFormatters
                .OfType<MetricsPrometheusTextOutputFormatter>()
                .First();
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
                DateTimeReset = DateTime.Now.Add(timeSpanInterval);
                return;
            }

            if (DateTime.Now > DateTimeReset)
            {
                var dateTimeResetNext = DateTimeReset?.Add(timeSpanInterval);
                if (DateTime.Now > dateTimeResetNext)
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

            var context = httpContextAccessor?.HttpContext;

            CheckClearContextsInterval();




            var metricsContextValueSource02 = metrics.Snapshot.GetForContext("Custom.Requests");

            //var firt = metricsRoot.Reporters.First();
            var value = metricsContextValueSource02.BucketTimers.FirstOrDefault();
            if (value != null)
            {

            }
        }

    }
}
