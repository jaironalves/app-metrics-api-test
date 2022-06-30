using App.Metrics;
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
        public static string ContextName = "Application.HttpClientRequests";

        public static class Counters
        {
            public static readonly CounterOptions InProgressRequestCount = new CounterOptions
            {
                Context = ContextName,
                Name = "InProgress",
                MeasurementUnit = Unit.Custom("In Progress Requests")
            };

            public static readonly CounterOptions RequestErrorTotalCount = new CounterOptions
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
            public static readonly GaugeOptions EndpointOneMinuteErrorPercentageRate = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static readonly GaugeOptions OneMinErrorPercentageRate = new GaugeOptions
            {
                Context = ContextName,
                Name = "One Minute Error Percentage Rate",
                MeasurementUnit = Unit.Requests
            };
        }

        public static class Meters
        {
            public static readonly MeterOptions CommandErrorRequestPerSessionRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Endpoint And Status Code",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions CommandErrorRequestRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static readonly MeterOptions ErrorRequestRate = new MeterOptions
            {
                Context = ContextName,
                Name = "Error Rate",
                MeasurementUnit = Unit.Requests
            };
        }

        public static class Timers
        {
            public static readonly TimerOptions EndpointRequestTransactionDuration = new TimerOptions
            {
                Context = ContextName,
                Name = "Transactions Per Endpoint",
                MeasurementUnit = Unit.Requests
            };

            public static readonly TimerOptions RequestTransactionDuration = new TimerOptions
            {
                Context = ContextName,
                Name = "Transactions",
                MeasurementUnit = Unit.Requests
            };
        }
    }
}
