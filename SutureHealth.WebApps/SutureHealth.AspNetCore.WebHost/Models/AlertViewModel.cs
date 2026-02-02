namespace SutureHealth.AspNetCore.Models
{
    public class AlertViewModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public ContextClass Style { get; set; }
        public bool CanClose { get; set; } = true;
        public bool Hidden { get; set; } = false;
        public int? FadeSeconds { get; set; } = null;
        public AlertLink Link { get; set; } = null;

        public enum ContextClass
        {
            Primary = 0,
            Secondary,
            Success,
            Danger,
            Warning,
            Info,
            Light,
            Dark
        }

        public class AlertLink
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string NavigationUrl { get; set; }
        }
    }
}
