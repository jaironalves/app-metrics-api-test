using App.Metrics;
using AppMetricsTest.API.Metrics.Internal;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsTest.API.Metrics.HttpClient
{
    public class RequestErrorMeterHandler : DelegatingHandler
    {
        private readonly IMetrics metrics;
        private readonly IRequestAccessor requestAccessor;
                
        public RequestErrorMeterHandler(IMetrics metrics, IRequestAccessor requestAccessor)
        {
            this.metrics = metrics;
            this.requestAccessor = requestAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    metrics.RecordRequestError(requestAccessor);
                }

                return response;
            }
            catch
            {
                metrics.RecordRequestError(requestAccessor);

                throw;
                //metrics.DecrementInProgressRequests(requestAccessor);
            }

            
        }
    }
}
