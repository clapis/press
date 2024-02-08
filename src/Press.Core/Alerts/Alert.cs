namespace Press.Core.Alerts
{
    public class Alert
    {
        public string Id { get; set; }
        public string Term { get; set; }
        public string NotifyEmail { get; set; }
        public DateTime? LastNotification { get; set; }
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}