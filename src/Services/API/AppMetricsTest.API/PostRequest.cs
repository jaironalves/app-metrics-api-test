using System;

namespace AppMetricsTest.API
{
    public class PostRequest
    {
        Guid Id { get; set; } = Guid.NewGuid();
        public string Command { get; set; }
        public string Session { get; set; }
        public string Description { get; set; }
        public bool Alternate { get; set; }
    }
}
