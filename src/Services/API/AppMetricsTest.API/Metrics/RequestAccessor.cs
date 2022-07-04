using System.Threading;

namespace AppMetricsTest.API.Metrics
{
    public class RequestAccessor : IRequestAccessor
    {
        private static readonly AsyncLocal<RequestHolder> requestCurrent = new AsyncLocal<RequestHolder>();

        public PostRequest? PostRequest
        {
            get
            {
                return requestCurrent.Value?.PostRequest;
            }
            set
            {
                var holder = requestCurrent.Value;
                if (holder != null)
                {
                    holder.PostRequest = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    requestCurrent.Value = new RequestHolder { PostRequest = value };
                }
            }
        }

        private sealed class RequestHolder
        {
            public PostRequest? PostRequest;
        }
    }
}
