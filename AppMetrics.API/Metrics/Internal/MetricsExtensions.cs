using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace AppMetricsTest.API.Metrics.Internal
{
    internal static class MetricsExtensions
    {
        private static MetricTags GetTags(IRequestAccessor accessor)
        {
            var post = accessor.PostRequest;

            return new MetricTags(
                new[] { "request", "session" },
                new[] { post.Command, post.Session });
        }

        public static void IncrementInProgressRequests(this IMetrics metrics, IRequestAccessor accessor)
        {            
            var tags = GetTags(accessor);

            metrics.Measure.Counter.Increment(HttpClientMetricsRegistry.Counters.InProgressRequestCount, tags);
        }        

        public static void DecrementInProgressRequests(this IMetrics metrics, IRequestAccessor accessor)
        {
            var tags = GetTags(accessor);
            metrics.Measure.Counter.Decrement(HttpClientMetricsRegistry.Counters.InProgressRequestCount, tags);
        }

        public static void RecordRequestError(this IMetrics metrics, IRequestAccessor accessor)
        {
            var post = accessor.PostRequest;

            CountOverallErrorRequestsBySession(metrics, post.Session);

            metrics.Measure.Meter.Mark(HttpClientMetricsRegistry.Meters.ErrorRequestRate);

            RecordCommandsRequestErrors(metrics, post.Session, post.Command);
            RecordOverallPercentageOfErrorRequests(metrics);
            RecordEndpointsPercentageOfErrorRequests(metrics, post.Command);
        }

        private static void CountOverallErrorRequestsBySession(IMetrics metrics, string session)
        {
            var tags = new MetricTags("session", session);
            metrics.Measure.Counter.Increment(HttpClientMetricsRegistry.Counters.RequestErrorTotalCount, tags);
        }

        private static ITimer EndpointRequestTimer(this IMetrics metrics, string routeTemplate)
        {
            var tags = new MetricTags("command", routeTemplate);
            
            return metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.EndpointRequestTransactionDuration, tags);
        }

        private static ITimer RequestTimer(this IMetrics metrics)
        {            
            return metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTransactionDuration);
        }

        private static void RecordCommandsRequestErrors(IMetrics metrics, string command, string session)
        {
            var commandErrorRequestTags = new MetricTags("command", command);
            metrics.Measure.Meter.Mark(HttpClientMetricsRegistry.Meters.CommandErrorRequestRate, commandErrorRequestTags);

            var commandErrorRequestPerSessionTags = new MetricTags(
                new[] { "command", "session" },
                new[] { command, session });

            metrics.Measure.Meter.Mark(
                HttpClientMetricsRegistry.Meters.CommandErrorRequestPerSessionRate,
                commandErrorRequestPerSessionTags);
        }

        private static void RecordEndpointsPercentageOfErrorRequests(IMetrics metrics, string routeTemplate)
        {
            var tags = new MetricTags("command", routeTemplate);

            var endpointsErrorRate = metrics.Provider.Meter.Instance(HttpClientMetricsRegistry.Meters.CommandErrorRequestRate, tags);
            var endpointsRequestTransactionTime = metrics.EndpointRequestTimer(routeTemplate);

            metrics.Measure.Gauge.SetValue(
                HttpClientMetricsRegistry.Gauges.EndpointOneMinuteErrorPercentageRate,
                tags,
                () => new HitPercentageGauge(endpointsErrorRate, endpointsRequestTransactionTime, m => m.OneMinuteRate));
        }

        private static void RecordOverallPercentageOfErrorRequests(IMetrics metrics)
        {
            var totalErrorRate = metrics.Provider.Meter.Instance(HttpClientMetricsRegistry.Meters.ErrorRequestRate);
            var overallRequestTransactionTime = metrics.RequestTimer();

            metrics.Measure.Gauge.SetValue(
                HttpClientMetricsRegistry.Gauges.OneMinErrorPercentageRate,
                () => new HitPercentageGauge(totalErrorRate, overallRequestTransactionTime, m => m.OneMinuteRate));
        }
    }
}
