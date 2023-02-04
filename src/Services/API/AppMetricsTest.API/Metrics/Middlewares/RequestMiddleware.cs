using App.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static AppMetricsTest.API.Metrics.Internal.HttpClientMetricsRegistry;

namespace AppMetricsTest.API.Metrics.Middlewares
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IMetrics metrics;
        private readonly ILogger<RequestMiddleware> logger;

        public RequestMiddleware(
            RequestDelegate next,
            IMetrics metrics,
            ILogger<RequestMiddleware> logger)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.metrics = metrics;
            this.logger = logger;
        }

        // ReSharper disable UnusedMember.Global
        public async Task Invoke(HttpContext context)       
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await next(context);
                return;
            }

            var startTime = metrics.Clock.Nanoseconds;
            var startMil = Stopwatch.GetTimestamp();

            try
            {
                await next(context);
            }
            finally 
            {
                var elapsed = metrics.Clock.Nanoseconds - startTime;
                var elapsedMs = GetElapsedMilliseconds(startMil, Stopwatch.GetTimestamp());

                logger.LogInformation("Nano Elapsed {0} - Mill Elapsed {}", elapsed, elapsedMs);

                TimeTransaction(context, elapsed);
            }
            
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        private void TimeTransaction(HttpContext context, long elapsed)
        {
            RoutePattern routePattern = null;

            var endother = context.GetEndpoint();

            var queryFeature = context.Features[typeof(Microsoft.AspNetCore.Http.Features.IHttpRequestFeature)]
                                           as Microsoft.AspNetCore.Http.Features.IHttpRequestFeature;

            var endpointFeature = context.Features[typeof(Microsoft.AspNetCore.Http.Features.IEndpointFeature)]
                                           as Microsoft.AspNetCore.Http.Features.IEndpointFeature;


            var endpoint = (endpointFeature?.Endpoint as RouteEndpoint);

            if (endpoint != null)
            {
                routePattern = endpoint.RoutePattern;
            }

            var routePath = string.Empty;
            if (routePattern != null)
                routePath = routePattern.RawText;

            var route = $"{context.Request.Method} {routePath}";

            var tagsAll = new MetricTags(
              new[] { "route", "http_status"},
              new[] { route, context.Response.StatusCode.ToString() });

            var bucketOptions = BucketHistograms.BucketTimers.RequestTransactionDuration(new double[] { 100, 250, 500, 1000, 2000, 3000 });
            var instance = metrics.Provider.BucketTimer.Instance(bucketOptions, tagsAll);

            instance.Record(elapsed, TimeUnit.Nanoseconds);
        }
    }
}
