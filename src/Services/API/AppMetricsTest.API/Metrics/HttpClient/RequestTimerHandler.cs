using App.Metrics;
using AppMetricsTest.API.Metrics.Internal;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsTest.API.Metrics.HttpClient
{
    public class RequestTimerHandler : DelegatingHandler
    {
        private readonly IMetrics metrics;
        private readonly IRequestAccessor requestAccessor;
        private readonly ILogger<RequestTimerHandler> logger;

        public RequestTimerHandler(IMetrics metrics, IRequestAccessor requestAccessor,
            ILogger<RequestTimerHandler> logger)
        {
            this.metrics = metrics;
            this.requestAccessor = requestAccessor;
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = metrics.Clock.Nanoseconds;
            var startMil = Stopwatch.GetTimestamp();

            try
            {
                return await base.SendAsync(request, cancellationToken);                
            }
            finally
            {
                var elapsed = metrics.Clock.Nanoseconds - startTime;
                var elapsedMs = GetElapsedMilliseconds(startMil, Stopwatch.GetTimestamp());

                logger.LogInformation("Nano Elapsed {0} - Mill Elapsed {}", elapsed, elapsedMs);

                metrics.RecordRequestTimer(requestAccessor, elapsed);
            }

            
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

    }
}
