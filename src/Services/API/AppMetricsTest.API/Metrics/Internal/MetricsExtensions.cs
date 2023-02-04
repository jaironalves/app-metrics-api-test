using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Meter;
using App.Metrics.Timer;
using System;

namespace AppMetricsTest.API.Metrics.Internal
{
    public static class MetricsExtensions
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
           
            RecordRequestErrors(metrics, post);
            RecordOneMinErrorPercentages(metrics, post);
        }

        public static void RecordRequestTimer(this IMetrics metrics, IRequestAccessor accessor, long elapsed)
        {
            var post = accessor.PostRequest;
            RecordRequestTimers(metrics, post, elapsed);
        }

        private static void RecordRequestTimers(IMetrics metrics, PostRequest postRequest, long elapsed)
        {
            var sessionTags = new MetricTags("session", postRequest.Session);
            var commandTags = new MetricTags("command", postRequest.Command);
            var sessionCommandTags = new MetricTags(
               new[] { "session", "command" },
               new[] { postRequest.Session, postRequest.Command });

            var tagsAll = new MetricTags(
               new[] { "session", "command", "alternate" },
               new[] { postRequest.Session, postRequest.Command, postRequest.Alternate.ToString().ToLower() });


            metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimer, tagsAll)
                .Record(elapsed, TimeUnit.Nanoseconds);

            //metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimer, commandTags)
            //    .Record(elapsed, TimeUnit.Nanoseconds);

            //metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimer, sessionTags)
            //   .Record(elapsed, TimeUnit.Nanoseconds);

            //metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimer, sessionCommandTags)
            //   .Record(elapsed, TimeUnit.Nanoseconds);

            metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimerSession, sessionTags)
               .Record(elapsed, TimeUnit.Nanoseconds);

            metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimerCommand, commandTags)
               .Record(elapsed, TimeUnit.Nanoseconds);

            metrics.Provider.Timer.Instance(HttpClientMetricsRegistry.Timers.RequestTimerSessionAndCommand, sessionCommandTags)
               .Record(elapsed, TimeUnit.Nanoseconds);
        }

        private static void CountOverallErrorRequestsBySession(IMetrics metrics, string session)
        {
            var tags = new MetricTags("session", session);
            metrics.Measure.Counter.Increment(HttpClientMetricsRegistry.Counters.SessionErrorTotalCount, tags);
        }

        private static void RecordRequestErrors(IMetrics metrics, PostRequest postRequest)
        {
            metrics.Measure.Meter.Mark(HttpClientMetricsRegistry.Meters.ErrorRequestRate);

            var sessionTags = new MetricTags("session", postRequest.Session);
            metrics.Measure.Meter.Mark(HttpClientMetricsRegistry.Meters.ErrorRequestRateSession, sessionTags);

            var commandTags = new MetricTags("command", postRequest.Command);
            metrics.Measure.Meter.Mark(HttpClientMetricsRegistry.Meters.ErrorRequestRateCommand, commandTags);

            var sessionCommandTags = new MetricTags(
                new[] { "session", "command" },
                new[] { postRequest.Session, postRequest.Command });

            metrics.Measure.Meter.Mark(
                HttpClientMetricsRegistry.Meters.ErrorRequestRateSessionAndCommand,
                sessionCommandTags);
        }

        private static void RecordOneMinErrorPercentages(IMetrics metrics, PostRequest postRequest)
        {
            var sessionTags = new MetricTags("session", postRequest.Session);
            var commandTags = new MetricTags("command", postRequest.Command);
            var sessionCommandTags = new MetricTags(
               new[] { "session", "command" },
               new[] { postRequest.Session, postRequest.Command });

            RecordOneMinErrorPercentage(metrics
                , HttpClientMetricsRegistry.Meters.ErrorRequestRate
                , HttpClientMetricsRegistry.Timers.RequestTimer
                , HttpClientMetricsRegistry.Gauges.OneMinErrorPercentageRate
                , new MetricTags());

            RecordOneMinErrorPercentage(metrics
                , HttpClientMetricsRegistry.Meters.ErrorRequestRateSession
                , HttpClientMetricsRegistry.Timers.RequestTimerSession
                , HttpClientMetricsRegistry.Gauges.OneMinErrorPercentageRateSession
                , sessionTags);

            RecordOneMinErrorPercentage(metrics
               , HttpClientMetricsRegistry.Meters.ErrorRequestRateCommand
               , HttpClientMetricsRegistry.Timers.RequestTimerCommand
               , HttpClientMetricsRegistry.Gauges.OneMinErrorPercentageRateCommand
               , commandTags);

            RecordOneMinErrorPercentage(metrics
               , HttpClientMetricsRegistry.Meters.ErrorRequestRateSessionAndCommand
               , HttpClientMetricsRegistry.Timers.RequestTimerSessionAndCommand
               , HttpClientMetricsRegistry.Gauges.OneMinErrorPercentageRateSessionAndCommand
               , sessionCommandTags);
        }

        private static void RecordOneMinErrorPercentage(IMetrics metrics
            , MeterOptions errorRateOptions
            , TimerOptions requestTimerOptions
            , GaugeOptions OneMinErrorPercentageOptions
            , MetricTags tags)
        {
            var errorRate = metrics.Provider.Meter.Instance(errorRateOptions, tags);
            var requestTimer = metrics.Provider.Timer.Instance(requestTimerOptions, tags);

            metrics.Measure.Gauge.SetValue(
                OneMinErrorPercentageOptions,
                tags,
                () => new HitPercentageGauge(errorRate, requestTimer, m => m.OneMinuteRate));
        }
    }
}
