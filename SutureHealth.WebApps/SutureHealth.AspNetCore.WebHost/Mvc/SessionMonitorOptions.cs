namespace SutureHealth.AspNetCore.Mvc
{
    public class SessionMonitorOptions
    {
        public TimeSpan CushionTimeout { get; set; }
        public bool Enable { get; set; } = true;
        public bool EnableUI { get; set; }
        public TimeSpan IdleTimeout { get; internal set; }
        public string LoginUrl { get; set; }
        public string PingUrl { get; set; }
        public TimeSpan WarningTimeout { get; set; }
    }
}
