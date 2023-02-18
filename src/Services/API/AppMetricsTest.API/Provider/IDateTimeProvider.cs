using System;

namespace AppMetricsTest.API.Provider
{
    public interface IDateTimeProvider
    {
        public DateTime Now { get;  }
    }
}
