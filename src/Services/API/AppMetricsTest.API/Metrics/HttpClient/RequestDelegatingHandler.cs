using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsTest.API.Metrics.HttpClient
{
    public class RequestDelegatingHandler : DelegatingHandler
    {
        private readonly IRequestAccessor requestAccessor;

        public RequestDelegatingHandler(IRequestAccessor requestAccessor)
        {
            this.requestAccessor = requestAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestJson = await request.Content.ReadAsStringAsync();
            var requestContent = JsonSerializer.Deserialize<PostRequest>(requestJson);

            requestContent.Description = "Passou no handler";

            requestAccessor.PostRequest = requestContent;

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
