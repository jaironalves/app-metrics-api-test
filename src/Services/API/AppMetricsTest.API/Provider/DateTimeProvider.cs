using System;

namespace AppMetricsTest.API.Provider
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
