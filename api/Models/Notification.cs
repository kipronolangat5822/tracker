namespace api.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationSeverity Severity { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public Guid? RelatedItemId { get; set; }
        public string RelatedItemType { get; set; } = string.Empty;
    }

    public enum NotificationType
    {
        EXPIRY_WARNING = 0,
        EXPIRY_URGENT = 1,
        EXPIRED = 2,
        LOW_STOCK = 3,
        GENERAL = 4
    }

    public enum NotificationSeverity
    {
        INFO = 0,
        WARNING = 1,
        ERROR = 2,
        CRITICAL = 3
    }
}
