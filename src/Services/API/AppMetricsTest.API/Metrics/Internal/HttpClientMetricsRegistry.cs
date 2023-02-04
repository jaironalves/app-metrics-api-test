using App.Metrics;
using App.Metrics.BucketHistogram;
using App.Metrics.BucketTimer;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Meter;
using App.Metrics.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppMetricsTest.API.Metrics.Internal
{
    internal static class HttpClientMetricsRegistry
    {
        public static string ContextName = "HttpClient.Requests";

        public static string ContextNameC = "Custom.Requests";

        public static class BucketHistograms
        {
            public static class BucketTimers
            {
                public static readonly Func<double[], BucketTimerOptions> EndpointRequestTransactionDuration = buckets => new BucketTimerOptions
                {
                    Context = ContextNameC,
                    Name = "Transactions Per Endpoint",
                    MeasurementUnit = Unit.Requests,
                    Buckets = buckets
                };

                public static readonly Func<double[], BucketTimerOptions> RequestTransactionDuration = buckets => new BucketTimerOptions
                {
                    Context = ContextNameC,
                    Name = "Transactions",
                    MeasurementUnit = Unit.Requests,
                    Buckets = buckets
                };
            }
        }

        public static class Counters
        {
            public static readonly CounterOptions InProgressRequestCount = new CounterOptions
            {
                Context = ContextName,
                Name = "InProgress",
                MeasurementUnit = Unit.Custom("In Progress Requests")
            };

            public static readonly CounterOptions SessionErrorTotalCount = new CounterOptions
            {
                Context = ContextName,
                Name = "Errors",
                ResetOnReporting = true,
                MeasurementUnit = Unit.Errors
            };

            public static readonly CounterOptions RequestErrorUnhandledExceptionCount = new CounterOptions
            {
                Context = ContextName,
                Name = "Exceptions",
                MeasurementUnit = Unit.Errors,
                ReportItemPercentages = false,
                ReportSetItems = false,
                ResetOnReporting = true
            };
        }

        public static class Gauges
        {
            public static readonly GaugeOptions OneMinErrorPercentageRate = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate",
                MeasurementUnit = Unit.Requests
            };

            public static readonly GaugeOptions OneMinErrorPercentageRateSession = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate Per Session",
                MeasurementUnit = Unit.Requests
            };

            public static readonly GaugeOptions OneMinErrorPercentageRateCommand = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate Per Command",
                MeasurementUnit = Unit.Requests
            };

            public static readonly GaugeOptions OneMinErrorPercentageRateSessionAndCommand = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate Per Session And Command",
                MeasurementUnit = Unit.Requests
            };

            //public static readonly GaugeOptions EndpointOneMinuteErrorPercentageRate = new GaugeOptions
            //{
            //    Context = ContextName,
            //    Name = "One Minute Error Percentage Rate Per Endpoint",
            //    MeasurementUnit = Unit.Requests
            //};           
        }

        public static class Meters
        {
            public static readonly MeterOptions ErrorRequestRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions ErrorRequestRateSession = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Session",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions ErrorRequestRateCommand = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Command",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions ErrorRequestRateSessionAndCommand = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Session And Command",
                MeasurementUnit = Unit.Requests
            };            
            
        }

        public static class Timers
        {
            public static readonly TimerOptions RequestTimer = new TimerOptions
            {
                Context = ContextName,
                Name = "Requests",
                MeasurementUnit = Unit.Requests
            };

            public static readonly TimerOptions RequestTimerSession = new TimerOptions
            {
                Context = ContextName,
                Name = "Requests Per Session",
                MeasurementUnit = Unit.Requests
            };

            public static readonly TimerOptions RequestTimerCommand = new TimerOptions
            {
                Context = ContextName,
                Name = "Requests Per Command",
                MeasurementUnit = Unit.Requests
            };

            public static readonly TimerOptions RequestTimerSessionAndCommand = new TimerOptions
            {
                Context = ContextName,
                Name = "Requests Session And Command",
                MeasurementUnit = Unit.Requests
            };

            //public static readonly TimerOptions EndpointRequestTransactionDuration = new TimerOptions
            //{
            //    Context = ContextName,
            //    Name = "Transactions Per Endpoint",
            //    MeasurementUnit = Unit.Requests
            //};

            //public static readonly TimerOptions RequestTransactionDuration = new TimerOptions
            //{
            //    Context = ContextName,
            //    Name = "Transactions",
            //    MeasurementUnit = Unit.Requests
            //};
        }
    }
}
