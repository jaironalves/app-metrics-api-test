namespace AppMetricsTest.API.Metrics.Options
{
    public class MetricCustomOptions
    {
        public string ContextName { get; private set; } = "AppContext";
        public string ContextNameReset { get; private set; }

        public void Update(string contextName, string contextNameReset)
        {
            ContextName = contextName;
            ContextNameReset = contextNameReset;
        }
    }
}
