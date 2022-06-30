using App.Metrics;
using AppMetricsTest.API.Metrics.Internal;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsTest.API.Metrics.HttpClient
{
    public class RequestInProgressHandler : DelegatingHandler
    {
        private readonly IMetrics metrics;
        private readonly IRequestAccessor requestAccessor;
                
        public RequestInProgressHandler(IMetrics metrics, IRequestAccessor requestAccessor)
        {
            this.metrics = metrics;
            this.requestAccessor = requestAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            metrics.IncrementInProgressRequests(requestAccessor);
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            finally
            {
                metrics.DecrementInProgressRequests(requestAccessor);
            }

            
        }
    }
}
