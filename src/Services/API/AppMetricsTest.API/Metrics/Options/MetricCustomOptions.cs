namespace AppMetricsTest.API.Metrics.Options
{
    public class MetricCustomOptions
    {
        public string ContextName { get; set; } = "AppContext";
        public string ContextNameReset { get; set; }

        public void Update(string contextName, string contextNameReset)
        {
            ContextName = contextName;
            ContextNameReset = contextNameReset;
        }
    }
}
