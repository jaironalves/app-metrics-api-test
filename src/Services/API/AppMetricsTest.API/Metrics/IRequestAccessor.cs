using System;

namespace AppMetricsTest.API.Metrics
{
    public interface IRequestAccessor
    {
        PostRequest? PostRequest { get; set; }
       
    }
}
